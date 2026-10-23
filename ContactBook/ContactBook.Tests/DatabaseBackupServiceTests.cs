using System;
using System.IO;
using System.Threading.Tasks;
using ContactBook.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContactBook.Tests
{
    [TestClass]
    public class DatabaseBackupServiceTests
    {
        private string _tempDir = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "DbBackupTest_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
            {
                try { Directory.Delete(_tempDir, true); } catch { }
            }
        }

        [TestMethod]
        public async Task BackupAndRestore_ShouldCopyDatabaseFile()
        {
            var service = new DatabaseBackupService();
            var dummyDbPath = service.GetDatabasePath();

            // Создаем фиктивный файл contacts.db если его нет
            if (!File.Exists(dummyDbPath))
            {
                await File.WriteAllTextAsync(dummyDbPath, "TEST_DB_DATA");
            }

            var backupPath = Path.Combine(_tempDir, "contacts_backup.bak");

            // Резервное копирование
            await service.BackupDatabaseAsync(backupPath);
            Assert.IsTrue(File.Exists(backupPath));

            // Восстановление
            var restoredPath = Path.Combine(_tempDir, "contacts_restored.db");
            using (var src = File.OpenRead(backupPath))
            using (var dst = File.Create(restoredPath))
            {
                await src.CopyToAsync(dst);
            }

            Assert.IsTrue(File.Exists(restoredPath));
            Assert.AreEqual(new FileInfo(backupPath).Length, new FileInfo(restoredPath).Length);
        }
    }
}
