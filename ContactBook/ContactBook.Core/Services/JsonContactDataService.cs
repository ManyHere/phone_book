using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
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
            using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // Заголовок таблицы
            await writer.WriteLineAsync("Id;FirstName;LastName;PhoneNumber;Email;BirthDate");

            foreach (var c in contacts)
            {
                var birthDateStr = c.BirthDate.HasValue ? c.BirthDate.Value.ToString("yyyy-MM-dd") : string.Empty;
                var line = $"{c.Id};\"{c.FirstName}\";\"{c.LastName}\";\"{c.PhoneNumber}\";\"{c.Email}\";{birthDateStr}";
                await writer.WriteLineAsync(line);
            }
        }

        public async Task<IReadOnlyList<Contact>> ImportFromCsvAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return Array.Empty<Contact>();
            }

            var result = new List<Contact>();
            using var reader = new StreamReader(filePath, Encoding.UTF8);

            string? header = await reader.ReadLineAsync();
            if (header == null)
            {
                return result;
            }

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(';');
                if (parts.Length < 4) continue;

                static string Clean(string val) => val.Trim().Trim('"');

                var contact = new Contact
                {
                    FirstName = Clean(parts.Length > 1 ? parts[1] : string.Empty),
                    LastName = Clean(parts.Length > 2 ? parts[2] : string.Empty),
                    PhoneNumber = Clean(parts.Length > 3 ? parts[3] : string.Empty),
                    Email = parts.Length > 4 && !string.IsNullOrWhiteSpace(Clean(parts[4])) ? Clean(parts[4]) : null,
                    BirthDate = parts.Length > 5 && DateTime.TryParse(Clean(parts[5]), out var dt) ? dt : null
                };

                result.Add(contact);
            }

            return result;
        }
    }
}
