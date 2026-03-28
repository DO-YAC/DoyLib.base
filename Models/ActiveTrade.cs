using DoyVestment.Framework.Models.Enums;

namespace doylib.Models;

public record ActiveTrade(
    TradeAction TradeAction,
    double? TP,
    double? SL);
