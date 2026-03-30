using DoyVestment.Framework.Models;

namespace doylib;

public interface ICandleWindowService
{
    void Initialize(int maxSize);
    void AddCandle(Candle candle);
    void AddCandle(Candle[] candles);
}