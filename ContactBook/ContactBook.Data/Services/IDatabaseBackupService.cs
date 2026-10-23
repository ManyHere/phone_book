using System.Threading.Tasks;

namespace ContactBook.Data.Services
{
    public interface IDatabaseBackupService
    {
        Task BackupDatabaseAsync(string destinationPath);
        Task RestoreDatabaseAsync(string sourceBackupPath);
        long GetDatabaseSizeBytes();
        string GetDatabasePath();
    }
}
