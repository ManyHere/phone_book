using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ContactBook.Core.Models;
using ContactBook.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContactBook.Tests
{
    [TestClass]
    public class JsonContactDataServiceTests
    {
        private string _tempDir = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "ContactBookTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [TestMethod]
        public async Task ExportAndImportJson_ShouldPreserveContactData()
        {
            // Arrange
            var service = new JsonContactDataService();
            var filePath = Path.Combine(_tempDir, "test_contacts.json");
            var original = new List<Contact>
            {
                new()
                {
                    Id = 1,
                    FirstName = "Иван",
                    LastName = "Иванов",
                    PhoneNumber = "+79991234567",
                    Email = "ivanov@test.com",
                    BirthDate = new DateTime(1990, 5, 15)
                },
                new()
                {
                    Id = 2,
                    FirstName = "Анна",
                    LastName = "Смирнова",
                    PhoneNumber = "+79997654321",
                    Email = "anna@test.com"
                }
            };

            // Act
            await service.ExportToJsonAsync(filePath, original);
            var imported = await service.ImportFromJsonAsync(filePath);

            // Assert
            Assert.IsNotNull(imported);
            Assert.HasCount(2, imported);
            Assert.AreEqual("Иван", imported[0].FirstName);
            Assert.AreEqual("Иванов", imported[0].LastName);
            Assert.AreEqual("+79991234567", imported[0].PhoneNumber);
            Assert.AreEqual("ivanov@test.com", imported[0].Email);
            Assert.AreEqual(new DateTime(1990, 5, 15), imported[0].BirthDate);
            Assert.AreEqual("Анна", imported[1].FirstName);
        }

        [TestMethod]
        public async Task ExportAndImportCsv_ShouldPreserveContactData()
        {
            // Arrange
            var service = new JsonContactDataService();
            var filePath = Path.Combine(_tempDir, "test_contacts.csv");
            var original = new List<Contact>
            {
                new()
                {
                    Id = 1,
                    FirstName = "Петр",
                    LastName = "Петров",
                    PhoneNumber = "+79001112233",
                    Email = "petr@test.com",
                    BirthDate = new DateTime(1985, 12, 1)
                }
            };

            // Act
            await service.ExportToCsvAsync(filePath, original);
            var imported = await service.ImportFromCsvAsync(filePath);

            // Assert
            Assert.IsNotNull(imported);
            Assert.HasCount(1, imported);
            Assert.AreEqual("Петр", imported[0].FirstName);
            Assert.AreEqual("Петров", imported[0].LastName);
            Assert.AreEqual("+79001112233", imported[0].PhoneNumber);
            Assert.AreEqual("petr@test.com", imported[0].Email);
            Assert.AreEqual(new DateTime(1985, 12, 1), imported[0].BirthDate);
        }

        [TestMethod]
        public async Task ImportFromJson_NonExistentFile_ReturnsEmptyList()
        {
            // Arrange
            var service = new JsonContactDataService();
            var nonExistentPath = Path.Combine(_tempDir, "absent.json");

            // Act
            var imported = await service.ImportFromJsonAsync(nonExistentPath);

            // Assert
            Assert.IsNotNull(imported);
            Assert.IsEmpty(imported);
        }

        [TestMethod]
        public async Task ImportFromCsv_NonExistentFile_ReturnsEmptyList()
        {
            // Arrange
            var service = new JsonContactDataService();
            var nonExistentPath = Path.Combine(_tempDir, "absent.csv");

            // Act
            var imported = await service.ImportFromCsvAsync(nonExistentPath);

            // Assert
            Assert.IsNotNull(imported);
            Assert.IsEmpty(imported);
        }
    }
}
