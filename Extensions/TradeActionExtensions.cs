
using DoyVestment.Framework.Models.Enums;

namespace doylib.Extensions;

public static class TradeActionExtensions
{
    public static TradeAction ToFrameWorkTradeAction(this Services.TradeAction action)
    {
        return action switch
        {
            Services.TradeAction._0 => TradeAction.NONE,
            Services.TradeAction._1 => TradeAction.BUY,
            Services.TradeAction._2 => TradeAction.SELL,
            Services.TradeAction.__1 => TradeAction.CLOSE,
            _ => TradeAction.NONE
        };
    }
}
