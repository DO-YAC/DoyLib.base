using System;
using doylib.Enums;
using doylib.Models;


namespace doylib.Strategy.Modules;


/// <summary>
/// A module only needs to implement <see cref="Evaluate"/> and <see cref="Warmup"/>.
/// If a module requires more than <see cref="Evaluate"/> and <see cref="Warmup"/>, it is already too complex.
/// </summary>
public class ExampleModule : IStrategyModule
{
    public string Name => "ExampleModule";
    private readonly Random mRandom = new();
    
    public TradeAction Evaluate(Line line)
    {
        var decision = mRandom.Next(0, 3);
        return (TradeAction)decision;
    }

    public void Warmup()
    {
    }
}