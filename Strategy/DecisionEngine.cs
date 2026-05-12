using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doylib.Ai.Interfaces;
using doylib.Logging;
using doylib.Strategy.Interfaces;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using Microsoft.Extensions.Logging;

namespace doylib.Strategy;

internal class DecisionEngine
{
    private readonly List<IStrategyModule> mModules = [];
    private readonly ILogger mLogger;
    private readonly double mQuorum;
    private readonly IAiInferenceService? mAi;

    internal DecisionEngine(DoyLibSettings settings, IAiInferenceService? ai = null)
    {
        mLogger = LoggerProvider.CreateLogger<DecisionEngine>();
        mQuorum = settings.Quorum;
        mAi = ai;
    }

    internal void Register(IStrategyModule module)
    {
        if (module is IAiStrategyModule aiModule && mAi != null)
        {
            aiModule.AttachAi(mAi);
        }
        mModules.Add(module);
    }

    internal TradeAction Evaluate()
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
                results[i] = mModules[i].Evaluate();
            }
            catch
            {
                mLogger.LogWarning("Module '{ModuleName}' failed to evaluate", mModules[i].Name);
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
