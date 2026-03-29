using DoyVestment.Framework.Models.Enums;

namespace doylib.Engine;

public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate();
    void Warmup();
}
