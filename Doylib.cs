using doylib.Engine.Modules;
using doylib.Logging;
using doylib.Services;
using doylib.Services.Interfaces;
using doylib.Strategy;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using DoyVestment.Framework.Services;
using Microsoft.Extensions.Logging;
using System;

namespace doylib;

public class Doylib
{
    private readonly ILogger mLogger;
    private readonly DecisionEngine mDecisionEngine;
    private readonly IActiveTradeHandler mActiveTradeHandler;

    // TODO: Add some kind of Containerization / DI to Doylib to avoid redundant class initializations.
    public Doylib(DoylibSettings settings)
    {
        mLogger = LoggerProvider.CreateLogger<Doylib>();
        mDecisionEngine = new DecisionEngine(settings);
        mDecisionEngine.Register(new ExampleModule());
        mActiveTradeHandler = new ActiveTradeHandler(
            new DoyExceptionHandler(
                new ProcessTerminationService()));
    }

    public DoyLibTradeResponse Execute(Candle candle)
    {
        if (mLogger.IsEnabled(LogLevel.Debug))
        {
            mLogger.LogDebug("Execute called");
        }

        mActiveTradeHandler.RemoveTpOrSlHit(candle);

        var decision = mDecisionEngine.Evaluate(candle);

        if (mLogger.IsEnabled(LogLevel.Debug))
        {
            mLogger.LogDebug("Got decision: {Decision}", decision);
        }

        DoyLibTradeResponse response;

        if (mActiveTradeHandler.ExampleHandle(decision, out var doyTradeId))
        {
            response = new DoyLibTradeResponse(doyTradeId!.Value, TradeAction.CLOSE, null, null);
            mActiveTradeHandler.RemoveActiveTrade(response.DoyTradeId);

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
}
