using DoyVestment.Framework.Models;

namespace doylib;

public interface ICandleWindowService
{
    int Count { get; }

    void Initialize(int maxSize);
    void AddCandle(Candle candle);
}