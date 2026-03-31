# DoyLib

Multi-strategy trade decision engine for the [DoyVestment](https://github.com/DO-YAC/DoyVestment.public) ecosystem.

DoyLib is the base library that powers trade decisions across the platform. It takes in candle data and runs it through a strategy engine where multiple customizable decision modules independently evaluate whether to buy, sell, or hold.

This repository is the **base configuration**, internally we use different versions of this.

## How It Works

1. Candle data comes in as 'Candle' object
2. All registered strategy modules evaluate the data in parallel
3. The engine tallies votes and applies a quorum threshold
4. If enough modules agree, the trade action is returned otherwise, no action is taken


## Strategy Modules

Implement `IStrategyModule` to create a new module:

```csharp
public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate();
    void Warmup();
}
```

- `Evaluate` — Returns `BUY`, `SELL`, or `NONE`
- `Warmup` — called once before the first evaluation (used primally for loading AI models)

New modules can be registered without restructuring the system.
