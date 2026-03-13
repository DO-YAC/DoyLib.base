using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doylib.Enums;
using doylib.Models;

namespace doylib.Strategy;

public class StrategyEngine
{
    private static readonly List<IStrategyModule> sModules = new();

    public static void Register(IStrategyModule module)
    {
        sModules.Add(module);
    }

    public static TradeAction Evaluate(Line line)
    {
        const double quorum = 0.5;
        var results = new TradeAction[sModules.Count];

        Parallel.For(0, sModules.Count, i =>
        {
            results[i] = sModules[i].Evaluate(line);
        });

        var topResult = results
            .CountBy(a => a)
            .MaxBy(a => a.Value);

        if ((double)topResult.Value / sModules.Count >= quorum)
        {
            return topResult.Key;
        }

        return TradeAction.NONE;
    }
}