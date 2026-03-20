using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;

namespace doylib.Engine;

public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate(Candle candle);
    void Warmup();
}
