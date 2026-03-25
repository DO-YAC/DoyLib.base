using doylib.Models;
using doylib.Services.Interfaces;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using DoyVestment.Framework.Services;
using DoyVestment.Framework.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace doylib.Services;

internal class ActiveTradeHandler : IActiveTradeHandler
{
    private readonly ConcurrentDictionary<Guid, ActiveTrade> mActiveTrades;
    private readonly IDoyExceptionHandler mDoyExceptionHandler;

    public IReadOnlyDictionary<Guid, ActiveTrade> ActiveTrades => mActiveTrades;

    public ActiveTradeHandler(IDoyExceptionHandler doyExHandler)
    {
        mActiveTrades = [];
        mDoyExceptionHandler = doyExHandler;
    }

    public void AddActiveTrade(DoyLibTradeResponse trade)
    {
        var activeTradeObj = new ActiveTrade(
            trade.TradeAction,
            trade.TP,
            trade.SL);

        if (mActiveTrades.TryAdd(trade.DoyTradeId, activeTradeObj))
        {
            return;
        }

        var exMessage = string.Format("Could not add Trade with TradeId '{0}' to registry." +
            " It is advised to closely watch it or close this trade manually.",
            trade.DoyTradeId);

        var ex = new ActiveTradeException(exMessage, HttpStatusCode.InternalServerError, ExceptionSeverityLevel.Inoperable);
        mDoyExceptionHandler.HandleException(ex, DoyExceptionHandler.DefaultLogger);
    }

    public void RemoveActiveTrade(Guid doyTradeId)
    {
        if (mActiveTrades.TryRemove(doyTradeId, out _))
        {
            return;
        }

        var exMessage = string.Format("Failed to remove Trade with tradeId '{0}' from the registry." +
            " Your strategy now might not work as expected. It is advised to check the backup and restart the application.",
            doyTradeId);

        var ex = new ActiveTradeException(exMessage, HttpStatusCode.InternalServerError, ExceptionSeverityLevel.Inoperable);
        mDoyExceptionHandler.HandleException(ex, DoyExceptionHandler.DefaultLogger);
    }

    public void RemoveTpOrSlHit(Candle candle)
    {
        foreach(var kvp in mActiveTrades
            .Where(kvp => 
                kvp.Value.TP != null 
                && kvp.Value.SL != null)
            .ToList())
        {
            var remove = TpOrSlHit(kvp.Value, candle);
            if (remove)
            {
                RemoveActiveTrade(kvp.Key);
            }
        }
    }

    /// <summary>
    /// THIS IS ONLY FOR DEMONSTRATION PURPOSES
    /// IT IS HIGHLY ADVISED TO CUSTOMIZE THIS FUNCTIONALITY
    /// </summary>
    /// <param name="action"></param>
    /// <param name="doyTradeId"></param>
    public bool ExampleHandle(TradeAction action, out Guid? doyTradeId)
    {
        if (action == TradeAction.NONE)
        {
            doyTradeId = null;
            return false;
        }

        var tradeToClose = ActiveTrades.Where(kvp => kvp.Value.TradeAction != action).Select(kvp => kvp.Key).First(); // TODO: In the rest of DoyVestment the closing of multiple trades needs to be supported.
        doyTradeId = tradeToClose;
        return true;
    }

    private static bool TpOrSlHit(ActiveTrade trade, Candle candle)
    {
        if (trade.TradeAction == TradeAction.BUY)
        {
            var tpHit = candle.High >= trade.TP;
            var slHit = candle.Low <= trade.SL;

            return tpHit || slHit;
        }
        else
        {
            var tpHit = candle.Low <= trade.TP;
            var slHit = candle.High >= trade.SL;

            return tpHit || slHit;
        }
    }
}
