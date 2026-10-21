using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContactBook.Core.Models;

namespace ContactBook.Core.Interfaces
{
    public interface IContactDataService
    {
        Task ExportToJsonAsync(string filePath, IEnumerable<Contact> contacts);
        Task<IReadOnlyList<Contact>> ImportFromJsonAsync(string filePath);
        Task ExportToCsvAsync(string filePath, IEnumerable<Contact> contacts);
        Task<IReadOnlyList<Contact>> ImportFromCsvAsync(string filePath);
    }
}
