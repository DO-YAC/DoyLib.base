using System;
using DoyVestment.Framework.Models;

namespace doylib.Services.Interfaces;

public interface ICandleWindowService
{
    void Initialize(int maxSize);
    void AddCandle(Candle candle);
    void AddCandle(Candle[] candles);
    ReadOnlyMemory<Candle> Window { get; }
}