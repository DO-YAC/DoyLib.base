using DoyVestment.Framework.Models.Enums;

namespace doylib.Strategy.Interfaces;

public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate();
    void Warmup();
}
