using doylib.Enums;
using doylib.Logging;
using doylib.Models;
using doylib.Strategy.Modules;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doylib.Strategy;

internal static class CombinedStrategyEngine
{
    private static readonly object sLockObject = new();
    private static readonly List<IStrategyModule> sModules = new();
    private static readonly ILogger sLogger = LoggerProvider.CreateLogger(nameof(CombinedStrategyEngine));
    private static bool sInitialised;

    public static void EnsureInitialised()
    {
        if (sInitialised)
        {
            return;
        }

        lock (sLockObject)
        {
            if (sInitialised)
            {
                return;
            }

            sLogger.LogInformation("ENABLE_STRATEGY={EnableRandomStrategy}, ENABLE_AI={EnableAi}",
                StrategyEnvironment.EnableRandomStrategy, StrategyEnvironment.EnableAi);

            if (StrategyEnvironment.EnableRandomStrategy)
            {
                sLogger.LogInformation("Adding RandomStrategyModule");
                sModules.Add(new RandomStrategyModule());
            }

            if (StrategyEnvironment.EnableAi)
            {
                sLogger.LogInformation("Adding AiStrategyModule");
                try
                {
                    sModules.Add(new AiStrategyModule());
                }
                catch (Exception ex)
                {
                    sLogger.LogError(ex, "Failed to add AiStrategyModule: {Message}", ex.Message);
                }
            }

            sLogger.LogInformation("Total modules registered: {ModuleCount}", sModules.Count);
            sInitialised = true;
        }
    }

    public static void RegisterModule(IStrategyModule module)
    {
        if (module is null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        lock (sLockObject)
        {
            sModules.Add(module);
        }
    }

    public static void Warmup()
    {
        EnsureInitialised();

        lock (sLockObject)
        {
            foreach (var module in sModules.OfType<IStrategyWarmup>())
            {
                module.Warmup();
            }
        }
    }

    public static TradeAction Evaluate(Line line)
    {
        EnsureInitialised();

        List<TradeAction> decisions;
        lock (sLockObject)
        {
            // Jedes Modul evaluieren Exceptions werden abgefangen und als None behandelt
            decisions = sModules.Select(module =>
            {
                try
                {
                    sLogger.LogDebug("Calling Evaluate on module: {ModuleName}", module.Name);
                    var result = module.Evaluate(line);
                    sLogger.LogDebug("Module {ModuleName} returned: {Result}", module.Name, result);
                    return result;
                }
                catch (Exception ex)
                {
                    sLogger.LogError(ex, "Module {ModuleName} threw exception: {Message}", module.Name, ex.Message);
                    return TradeAction.NONE;
                }
            })
            // Nur echte Signale weiterbetrachten
            .Where(result => result != TradeAction.NONE)
            .ToList();
        }

        if (decisions.Count == 0)
        {
            // Kein Modul liefert ein gültiges Signal -> nicht handeln
            return TradeAction.NONE;
        }

        if (!StrategyEnvironment.RequireConsensus)
        {
            // Kein Konsens nötig: erstes Signal gewinnt
            return decisions[0];
        }

        if (decisions.All(result => result == decisions[0]))
        {
            // Konsensmodus: alle Module sind sich einig -> handeln
            return decisions[0];
        }
        // Uneinigkeit zwischen Modulen -> Sicherheit vor Aktion
        return TradeAction.NONE;
    }

    public static IReadOnlyList<string> GetActiveModuleNames()
    {
        EnsureInitialised();

        lock (sLockObject)
        {
            return sModules
                .Select(module => module.Name)
                .ToArray();
        }
    }
}
