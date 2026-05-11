using System.Linq;
using doylib.Ai.Interfaces;
using doylib.Services.Interfaces;
using doylib.Strategy.Interfaces;
using DoyVestment.Framework.Models.Enums;

namespace doylib.Strategy.Modules;

internal class ExampleAiModule(ICandleWindowService candleWindowService) : IAiStrategyModule
{
    private IAiSession? mSession;
    public string Name => "ExampleAiModule";
    
    public TradeAction Evaluate()
    {
        var window = candleWindowService.Window.Span;
        
        const int lookback = 60;
        const int features = 4;
        
        var slice = window.Slice(window.Length - lookback, lookback);

        var input = new float[lookback * features]; // TODO: add caching, this happens per tick 
        for (int i = 0; i < lookback; i++)
        {
            var candle = slice[i];
            var baseIndex = i * features;
            input[baseIndex] = (float)candle.Open;
            input[baseIndex+1] = (float)candle.High;
            input[baseIndex+2] = (float)candle.Low;
            input[baseIndex+3] = (float)candle.Close;
        }

        var shape = new long[] { 1, lookback, features };
        var logits = mSession.Run(mSession?.Inputs.Keys.First(), input, shape);
        
        // TODO: Logits -> Signal
        return TradeAction.NONE;
    }
    
    public void Warmup()
    {
    }
    
    public void AttachAi(IAiInferenceService ai)
    {
        mSession = ai.GetSession("lstm_eurusd_m1");   // set in appsettings.json
    }
}