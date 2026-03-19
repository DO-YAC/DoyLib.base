using doylib.Enums;
using doylib.Models;

namespace doylib.Engine;

public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate(Line line);
    void Warmup();
}
