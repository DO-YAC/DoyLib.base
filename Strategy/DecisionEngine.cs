using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doylib.Enums;
using doylib.Logging;
using doylib.Models;
using Microsoft.Extensions.Logging;

namespace doylib.Engine;

internal class DecisionEngine
{
    private static readonly List<IStrategyModule> sModules = new();
    private static readonly ILogger sLogger = LoggerProvider.CreateLogger<DecisionEngine>();

    internal static void Register(IStrategyModule module)
    {
        sModules.Add(module);
    }

    internal static TradeAction Evaluate(Line line)
    {
        if (sModules.Count == 0)
        {
            return TradeAction.NONE;
        }

        const double quorum = 0.5;
        var results = new TradeAction[sModules.Count];

        Parallel.For(0, sModules.Count, i =>
        {
            try
            {
                results[i] = sModules[i].Evaluate(line);
            }
            catch
            {
                results[i] = TradeAction.NONE;
            }
        });

        var topResult = results
            .CountBy(a => a)
            .MaxBy(a => a.Value);

        if ((double)topResult.Value / sModules.Count > quorum)
        {
            return topResult.Key;
        }

        return TradeAction.NONE;
    }

    internal static void Warmup()
    {
        Parallel.ForEach(sModules, module =>
            {
                try
                {
                    module.Warmup();
                }
                catch (Exception ex)
                {
                    sLogger.LogWarning(ex, "Warmup failed for module '{ModuleName}'", module.Name);
                }
            }
        );
    }

    internal static string[] GetActiveModules()
    {
        return [.. sModules.Select(module => module.Name)];
    }
}