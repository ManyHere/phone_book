using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ContactBook.Core.Models;

namespace ContactBook.Core.Interfaces
{
    public interface IContactRepository
    {
        Task<IReadOnlyList<Contact>> GetAllAsync();
        Task<Contact?> GetByIdAsync(int id);
        Task<IReadOnlyList<Contact>> SearchAsync(string query);
        Task<IReadOnlyList<Contact>> GetByGroupAsync(int groupId);
        Task<IReadOnlyList<Group>> GetAllGroupsAsync();
        Task AddAsync(Contact contact);
        Task UpdateAsync(Contact contact);
        Task DeleteAsync(int id);
    }
}
