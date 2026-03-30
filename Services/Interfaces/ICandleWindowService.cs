using System;
using DoyVestment.Framework.Models;

namespace doylib;

public interface ICandleWindowService
{
    int Count { get; }
    DateTime? LatestTimestamp { get; }

    void Initialize(int maxSize);
    void AddCandle(Candle candle);
    void AddCandle(Candle[] candles);
}