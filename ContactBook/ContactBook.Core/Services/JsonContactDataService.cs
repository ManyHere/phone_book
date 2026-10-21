using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ContactBook.Core.Interfaces;
using ContactBook.Core.Models;

namespace ContactBook.Core.Services
{
    public class JsonContactDataService : IContactDataService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
        };

        public async Task ExportToJsonAsync(string filePath, IEnumerable<Contact> contacts)
        {
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, contacts, JsonOptions);
        }

        public async Task<IReadOnlyList<Contact>> ImportFromJsonAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Array.Empty<Contact>();
            }

            using var stream = File.OpenRead(filePath);
            var contacts = await JsonSerializer.DeserializeAsync<List<Contact>>(stream, JsonOptions);
            return contacts ?? (IReadOnlyList<Contact>)Array.Empty<Contact>();
        }

        public async Task ExportToCsvAsync(string filePath, IEnumerable<Contact> contacts)
        {
            using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);

            // Заголовок таблицы
            await writer.WriteLineAsync("Id;FirstName;LastName;PhoneNumber;Email;BirthDate");

            foreach (var c in contacts)
            {
                var birthDateStr = c.BirthDate.HasValue ? c.BirthDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                var line = $"{c.Id};\"{c.FirstName}\";\"{c.LastName}\";\"{c.PhoneNumber}\";\"{c.Email}\";{birthDateStr}";
                await writer.WriteLineAsync(line);
            }
        }
    }
}