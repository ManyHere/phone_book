using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ContactBook.Core.Interfaces;
using ContactBook.Core.Models;

namespace ContactBook.Data
{
    public class ContactRepository : IContactRepository
    {
        private readonly ContactDbContext _context;

        public ContactRepository(ContactDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IReadOnlyList<Contact>> GetAllAsync()
        {
            return await _context.Contacts
                .Include(c => c.Group)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(int id)
        {
            return await _context.Contacts
                .Include(c => c.Group)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IReadOnlyList<Contact>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await GetAllAsync();
            }

            query = query.Trim();
            var contacts = await _context.Contacts
                .Include(c => c.Group)
                .AsNoTracking()
                .ToListAsync();

            return contacts
                .Where(c => c.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || c.LastName.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || c.PhoneNumber.Contains(query, StringComparison.OrdinalIgnoreCase)
                         || (c.Email != null && c.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public async Task<IReadOnlyList<Contact>> GetByGroupAsync(int groupId)
        {
            return await _context.Contacts
                .Include(c => c.Group)
                .AsNoTracking()
                .Where(c => c.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Group>> GetAllGroupsAsync()
        {
            return await _context.Groups
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Contact contact)
        {
            ArgumentNullException.ThrowIfNull(contact);

            if (contact.Group != null && contact.GroupId.HasValue)
            {
                contact.Group = null;
            }

            await _context.Contacts.AddAsync(contact);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Contact contact)
        {
            ArgumentNullException.ThrowIfNull(contact);

            var existing = await _context.Contacts.FindAsync(contact.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Контакт с идентификатором {contact.Id} не найден.");
            }

            existing.FirstName = contact.FirstName;
            existing.LastName = contact.LastName;
            existing.PhoneNumber = contact.PhoneNumber;
            existing.Email = contact.Email;
            existing.BirthDate = contact.BirthDate;
            existing.Notes = contact.Notes;
            existing.Photo = contact.Photo;
            existing.GroupId = contact.GroupId;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }
    }
}
