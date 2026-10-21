using System;
using System.Windows.Forms;
using ContactBook.Core.Interfaces;
using ContactBook.Core.Services;
using ContactBook.Data;

namespace ContactBook.UI
{
    internal static class Program
    {
        /// <summary>
        ///  Главная точка входа для приложения.
        ///  Выполняет роль Composition Root (корень композиции зависимостей).
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 1. Инициализируем контекст SQLite и гарантируем создание таблиц
            using var dbContext = new ContactDbContext();
            dbContext.Database.EnsureCreated();

            // 2. Создаем экземпляры репозитория и сервиса экспорта/импорта
            IContactRepository repository = new ContactRepository(dbContext);
            IContactDataService dataService = new JsonContactDataService();

            // 3. Внедряем зависимости в Form1 через конструктор
            Application.Run(new Form1(repository, dataService));
        }
    }
}
