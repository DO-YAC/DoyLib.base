using doylib.Extensions;
using doylib.Services.Interfaces;
using DoyVestment.Framework.Models;
using System.Linq;
using System.Threading.Tasks;

namespace doylib.Services;

public class BackupService(ITradeBackupClient backupClient, IActiveTradeHandler activeTradeHandler) : IBackupService
{
    public async Task RestoreActiveTrades()
    {
        var activeTrades = await backupClient.BackupGetAsync();
        var typedActiveTrades = activeTrades.Result.ActiveTrades.Select(trade =>
        {
            return new DoyLibTradeResponse(trade.DoyTradeId, trade.TradeAction.ToFrameWorkTradeAction(), trade.Tp, trade.Sl);
        });

        foreach (var trade in typedActiveTrades)
        {
            activeTradeHandler.AddActiveTrade(trade);
        }
    }
}
