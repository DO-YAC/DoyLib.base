using doylib.Enums;
using doylib.Models;

namespace doylib.Strategy;

public class StrategyEngine
{
    public static TradeAction Evaluate(Line line)
    {
        
        return TradeAction.BUY;
    }
}