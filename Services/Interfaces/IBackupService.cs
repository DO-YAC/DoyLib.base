using System.Threading.Tasks;

namespace doylib.Services.Interfaces
{
    public interface IBackupService
    {
        Task RestoreActiveTrades();
    }
}