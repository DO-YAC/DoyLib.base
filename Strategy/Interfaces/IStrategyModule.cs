using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;

namespace doylib.Engine;

public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate(Candle candle, ICandleWindowService candleWindow);
    void Warmup();
}
