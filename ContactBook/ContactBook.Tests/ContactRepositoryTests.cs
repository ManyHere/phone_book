using System;
using System.IO;
using System.Threading.Tasks;
using ContactBook.Core.Models;
using ContactBook.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ContactBook.Tests
{
    [TestClass]
    public class ContactRepositoryTests
    {
        private string _dbPath = string.Empty;
        private ContactDbContext _context = null!;
        private ContactRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"contacts_test_{Guid.NewGuid():N}.db");
            var options = new DbContextOptionsBuilder<ContactDbContext>()
                .UseSqlite($"Data Source={_dbPath}")
                .Options;

            _context = new ContactDbContext(options);
            _context.Database.EnsureCreated();
            _repository = new ContactRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();

            if (File.Exists(_dbPath))
            {
                try { File.Delete(_dbPath); } catch { }
            }
        }

        [TestMethod]
        public async Task GetAllGroupsAsync_ShouldReturnSeededGroups()
        {
            var groups = await _repository.GetAllGroupsAsync();

            Assert.IsNotNull(groups);
            Assert.IsGreaterThanOrEqualTo(groups.Count, 5);
        }

        [TestMethod]
        public async Task AddAndGetByIdAsync_ShouldStoreContact()
        {
            var contact = new Contact
            {
                FirstName = "Алексей",
                LastName = "Смирнов",
                PhoneNumber = "+79123456789",
                Email = "alex@test.com",
                GroupId = 1
            };

            await _repository.AddAsync(contact);

            var loaded = await _repository.GetByIdAsync(contact.Id);
            Assert.IsNotNull(loaded);
            Assert.AreEqual("Алексей", loaded.FirstName);
            Assert.AreEqual("Смирнов", loaded.LastName);
            Assert.AreEqual("+79123456789", loaded.PhoneNumber);
            Assert.AreEqual(1, loaded.GroupId);
            Assert.IsNotNull(loaded.Group);
            Assert.AreEqual("Общие", loaded.Group.Name);
        }

        [TestMethod]
        public async Task SearchAsync_ShouldFindByFirstNameOrPhone()
        {
            await _repository.AddAsync(new Contact { FirstName = "Мария", LastName = "Кузнецова", PhoneNumber = "+79011112233" });
            await _repository.AddAsync(new Contact { FirstName = "Дмитрий", LastName = "Попов", PhoneNumber = "+79022223344" });

            var resultByName = await _repository.SearchAsync("мария");
            Assert.HasCount(1, resultByName);
            Assert.AreEqual("Мария", resultByName[0].FirstName);

            var resultByPhone = await _repository.SearchAsync("22233");
            Assert.HasCount(1, resultByPhone);
            Assert.AreEqual("Дмитрий", resultByPhone[0].FirstName);
        }

        [TestMethod]
        public async Task UpdateAsync_ShouldModifyExistingContact()
        {
            var contact = new Contact { FirstName = "Иван", LastName = "Иванов", PhoneNumber = "+79991112233" };
            await _repository.AddAsync(contact);

            contact.FirstName = "Иван Иванович";
            contact.PhoneNumber = "+79998887766";
            await _repository.UpdateAsync(contact);

            var updated = await _repository.GetByIdAsync(contact.Id);
            Assert.IsNotNull(updated);
            Assert.AreEqual("Иван Иванович", updated.FirstName);
            Assert.AreEqual("+79998887766", updated.PhoneNumber);
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldRemoveContact()
        {
            var contact = new Contact { FirstName = "Тест", LastName = "Удаляемый", PhoneNumber = "+70000000000" };
            await _repository.AddAsync(contact);

            await _repository.DeleteAsync(contact.Id);

            var loaded = await _repository.GetByIdAsync(contact.Id);
            Assert.IsNull(loaded);
        }
    }
}
