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
    private readonly List<IStrategyModule> mModules = new();
    private readonly ILogger mLogger;
    private readonly double mQuorum;

    internal DecisionEngine(DoylibSettings settings)
    {
        mLogger = LoggerProvider.CreateLogger<DecisionEngine>();
        mQuorum = settings.Quorum;
    }

    internal void Register(IStrategyModule module)
    {
        mModules.Add(module);
    }

    internal TradeAction Evaluate(Line line)
    {
        if (mModules.Count == 0)
        {
            return TradeAction.NONE;
        }

        var results = new TradeAction[mModules.Count];

        Parallel.For(0, mModules.Count, i =>
        {
            try
            {
                results[i] = mModules[i].Evaluate(line);
            }
            catch
            {
                results[i] = TradeAction.NONE;
            }
        });

        var topResult = results
            .CountBy(a => a)
            .MaxBy(a => a.Value);

        if ((double)topResult.Value / mModules.Count > mQuorum)
        {
            return topResult.Key;
        }

        return TradeAction.NONE;
    }

    internal void Warmup()
    {
        Parallel.ForEach(mModules, module =>
            {
                try
                {
                    module.Warmup();
                }
                catch (Exception ex)
                {
                    mLogger.LogWarning(ex, "Warmup failed for module '{ModuleName}'", module.Name);
                }
            }
        );
    }

    internal string[] GetActiveModules()
    {
        return [.. mModules.Select(module => module.Name)];
    }
}
