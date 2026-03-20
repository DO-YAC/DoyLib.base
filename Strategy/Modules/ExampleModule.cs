using System;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;

namespace doylib.Engine.Modules;

internal class ExampleModule : IStrategyModule
{
    public string Name => "ExampleModule";

    public TradeAction Evaluate(Candle candle)
    {
        var decision = Random.Shared.Next(0, 3);
        return (TradeAction)decision;
    }
    
    /// <summary>
    /// Called once before the first <see cref="Evaluate"/> invocation.
    /// Use this to validate AI model dimensions or pre-compute strategy indicators.
    /// </summary>
    public void Warmup()
    {
    }
}