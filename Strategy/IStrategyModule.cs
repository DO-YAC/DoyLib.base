using doylib.Enums;
using doylib.Models;

namespace doylib.Strategy;

public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate(Line line);
}
