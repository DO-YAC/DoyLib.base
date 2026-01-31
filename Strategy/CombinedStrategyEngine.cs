using doylib.Enums;
using doylib.Models;
using doylib.Strategy.Modules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace doylib.Strategy;

internal static class CombinedStrategyEngine
{
    private static readonly object sLockObject = new();
    private static readonly List<IStrategyModule> sModules = new();
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

            Console.WriteLine($"[DEBUG] ENABLE_STRATEGY={StrategyEnvironment.EnableRandomStrategy}, ENABLE_AI={StrategyEnvironment.EnableAi}");

            if (StrategyEnvironment.EnableRandomStrategy)
            {
                Console.WriteLine("[DEBUG] Adding RandomStrategyModule");
                sModules.Add(new RandomStrategyModule());
            }

            if (StrategyEnvironment.EnableAi)
            {
                Console.WriteLine("[DEBUG] Adding AiStrategyModule");
                try
                {
                    sModules.Add(new AiStrategyModule());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to add AiStrategyModule: {ex.Message}");
                }
            }

            Console.WriteLine($"[DEBUG] Total modules registered: {sModules.Count}");
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
                    Console.WriteLine($"[DEBUG] Calling Evaluate on module: {module.Name}");
                    var result = module.Evaluate(line);
                    Console.WriteLine($"[DEBUG] Module {module.Name} returned: {result}");
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Module {module.Name} threw exception: {ex.Message}");
                    Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
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
