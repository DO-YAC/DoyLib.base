using System;
using doylib.Services.Interfaces;
using doylib.Strategy.Interfaces;
using DoyVestment.Framework.Models.Enums;

namespace doylib.Strategy.Modules;

internal class ExampleModule(ICandleWindowService candleWindowService) : IStrategyModule
{
    public string Name => "ExampleModule";

    public TradeAction Evaluate()
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