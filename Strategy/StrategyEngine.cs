using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using doylib.Enums;
using doylib.Models;

namespace doylib.Strategy;

public class StrategyEngine
{
    public static TradeAction Evaluate(Line line, List<IStrategyModule> modules)
    {
        const double quorum = 0.5;
        var results = new TradeAction[modules.Count];
        
        Parallel.For(0, modules.Count, i =>
        {
            results[i] = modules[i].Evaluate(line);
        });

        var topResult = results
            .CountBy(a => a)
            .MaxBy(a => a.Value);
        
        if ((double)topResult.Value / modules.Count >= quorum)
        {
            return topResult.Key;
        }

        return TradeAction.NONE;
    }
}