using System;
using doylib.Enums;
using doylib.Models;

namespace doylib.Strategy.Modules;

public sealed class RandomStrategyModule : IStrategyModule
{
    private readonly Random mRandom = new();

    public string Name => "RandomStrategy";

    public TradeAction Evaluate(Line line)
    {
        // Random decision for testing
        var decision = mRandom.Next(0, 3);
        return decision switch
        {
            1 => TradeAction.BUY,
            2 => TradeAction.SELL,
            0 => TradeAction.NONE
        };
    }
}