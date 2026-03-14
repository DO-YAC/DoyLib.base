using System;
using doylib.Enums;
using doylib.Models;

namespace doylib.Engine.Modules;

/// <summary>
/// A module only needs to implement <see cref="Evaluate"/> and <see cref="Warmup"/>.
/// If a module requires more than <see cref="Evaluate"/> and <see cref="Warmup"/>, it is already too complex.
/// </summary>
internal class ExampleModule : IStrategyModule
{
    public string Name => "ExampleModule";
    private readonly Random mRandom = new();

    public TradeAction Evaluate(Line line)
    {
        var decision = mRandom.Next(0, 3);
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