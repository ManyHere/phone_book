using System;
using System.IO;
using System.Threading.Tasks;

namespace ContactBook.Data.Services
{
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private const string DefaultDbFileName = "contacts.db";

        public string GetDatabasePath() => Path.GetFullPath(DefaultDbFileName);

        public long GetDatabaseSizeBytes()
        {
            var path = GetDatabasePath();
            return File.Exists(path) ? new FileInfo(path).Length : 0;
        }

        public async Task BackupDatabaseAsync(string destinationPath)
        {
            var sourcePath = GetDatabasePath();
            if (!File.Exists(sourcePath))
            {
                throw new FileNotFoundException("Файл базы данных не найден.", sourcePath);
            }

            var destDir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(destinationStream);
        }

        public async Task RestoreDatabaseAsync(string sourceBackupPath)
        {
            if (!File.Exists(sourceBackupPath))
            {
                throw new FileNotFoundException("Файл резервной копии не найден.", sourceBackupPath);
            }

            var destPath = GetDatabasePath();
            using var sourceStream = new FileStream(sourceBackupPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destinationStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(destinationStream);
        }
    }
}
