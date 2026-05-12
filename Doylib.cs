using System;
using doylib.Ai;
using doylib.Ai.Interfaces;
using doylib.Logging;
using doylib.Services;
using doylib.Services.Interfaces;
using doylib.Strategy;
using doylib.Strategy.Modules;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using DoyVestment.Framework.Services;
using DoyVestment.Framework.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace doylib;

public class Doylib : IStrategy, IDisposable
{
    private readonly ILogger mLogger;
    private readonly IDoyExceptionHandler mDoyExceptionHandler;
    private readonly ICandleWindowService mCandleWindowService;
    private readonly DecisionEngine mDecisionEngine;
    private readonly IActiveTradeHandler mActiveTradeHandler;
    private readonly IAiInferenceService? mAiInferenceService;

    private event EventHandler<Guid> mTradeClosedSuccessfully;

    // TODO: Add some kind of Containerization / DI to Doylib to avoid redundant class initializations.
    public Doylib(DoyLibSettings settings)
    {
        mLogger = LoggerProvider.CreateLogger<Doylib>();
        mDoyExceptionHandler = new DoyExceptionHandler(new ProcessTerminationService());
        mCandleWindowService = new CandleWindowService(LoggerProvider.CreateLogger<CandleWindowService>(), mDoyExceptionHandler);
        mCandleWindowService.Initialize(settings.MaxCandleWindowSize);
        mActiveTradeHandler = new ActiveTradeHandler(mDoyExceptionHandler);

        if (settings.Ai != null && settings.Ai.Enabled == true)
        {
            var aiSettings = settings.Ai;
            mAiInferenceService = new OnnxInferenceService(aiSettings);
        }

        mDecisionEngine = new DecisionEngine(settings, mAiInferenceService);

        mDecisionEngine.Register(new ExampleModule(mCandleWindowService));
        mDecisionEngine.Register(new ExampleAiModule(mCandleWindowService));

        mTradeClosedSuccessfully += mActiveTradeHandler.OnTradeClosedSuccessfully;
    }

    public DoyLibTradeResponse Execute(Candle candle)
    {
        if (mLogger.IsEnabled(LogLevel.Debug))
        {
            mLogger.LogDebug("Execute called");
        }

        mCandleWindowService.AddCandle(candle);

        mActiveTradeHandler.RemoveTpOrSlHit(candle);

        var decision = mDecisionEngine.Evaluate();

        if (mLogger.IsEnabled(LogLevel.Debug))
        {
            mLogger.LogDebug("Got decision: {Decision}", decision);
        }

        DoyLibTradeResponse response;

        if (mActiveTradeHandler.ExampleHandle(decision, out var doyTradeId))
        {
            response = new DoyLibTradeResponse(doyTradeId!.Value, TradeAction.CLOSE, null, null);

            return response;

        }
        
        response = new DoyLibTradeResponse(
            Guid.NewGuid(),
            decision,
            null,
            null);

        mActiveTradeHandler.AddActiveTrade(response);

        return response;
    }

    public void Warmup()
    {
        mDecisionEngine.Warmup();
    }

    public string[] GetActiveModules()
    {
        return mDecisionEngine.GetActiveModules();
    }
    
    public void AddCandle(Candle[] candles)
    {
        mCandleWindowService.AddCandle(candles);
    }

    public void FireTradeClosedSuccessfully(Guid doyTradeId)
    {
        mTradeClosedSuccessfully.Invoke(this, doyTradeId);
    }
    
    public void Dispose()
    {
        mAiInferenceService?.Dispose();
    }
}
