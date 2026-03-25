using doylib.Models;
using DoyVestment.Framework.Models;
using DoyVestment.Framework.Models.Enums;
using System;
using System.Collections.Generic;

namespace doylib.Services.Interfaces;

internal interface IActiveTradeHandler
{
    IReadOnlyDictionary<Guid, ActiveTrade> ActiveTrades { get; }

    void RemoveActiveTrade(Guid doyTradeId);

    void AddActiveTrade(DoyLibTradeResponse trade);

    void RemoveTpOrSlHit(Candle candle);

    /// <summary>
    /// THIS IS ONLY FOR DEMONSTRATION PURPOSES
    /// IT IS HIGHLY ADVISED TO CUSTOMIZE THIS FUNCTIONALITY
    /// </summary>
    /// <param name="action"></param>
    /// <param name="doyTradeId"></param>
    bool ExampleHandle(TradeAction action, out Guid? doyTradeId);
}
