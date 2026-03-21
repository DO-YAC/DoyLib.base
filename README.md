# DoyLib

Multi-strategy trade decision engine for the [DoyVestment](https://github.com/DO-YAC/DoyVestment.public) ecosystem.

DoyLib is the base library that powers trade decisions across the platform. It takes in candle data and runs it through a strategy engine where multiple decision modules (AI-based, rule-based, or both) independently evaluate whether to buy, sell, or hold.

This repository is the **base configuration**, internally we use different Versions of this.

## How It Works

1. Candle data comes in as JSON
2. All registered strategy modules evaluate the candle in parallel
3. The engine tallies votes and applies a quorum threshold
4. If enough modules agree, the trade action is returned otherwise, no action is taken


## Strategy Modules

Implement `IStrategyModule` to create a new module:

```csharp
public interface IStrategyModule
{
    string Name { get; }
    TradeAction Evaluate(Candle candle);
    void Warmup();
}
```

- `Evaluate` — receives a candle and returns `BUY`, `SELL`, or `NONE`
- `Warmup` — called once before the first evaluation (used primally for loading AI models)

New modules can be registered without restructuring the system.
