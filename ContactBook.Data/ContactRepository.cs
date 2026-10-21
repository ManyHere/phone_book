using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using ContactBook.Core;

namespace ContactBook.Data
{
    public class ContactRepository
    {
        public List<Contact> GetAllContacts()
        {
            using (var db = new ContactDbContext())
            {
                return db.Contacts.ToList();
            }
        }

        public void AddContact(Contact contact)
        {
            using (var db = new ContactDbContext())
            {
                db.Contacts.Add(contact);
                db.SaveChanges();
            }
        }

        public void UpdateContact(Contact contact)
        {
            using (var db = new ContactDbContext())
            {
                db.Contacts.Update(contact);
                db.SaveChanges();
            }
        }

        public void DeleteContact(int id)
        {
            using (var db = new ContactDbContext())
            {
                var contact = db.Contacts.Find(id);
                if (contact != null)
                {
                    db.Contacts.Remove(contact);
                    db.SaveChanges();
                }
            }
        }

        public List<Contact> SearchContacts(string query)
        {
            using (var db = new ContactDbContext())
            {
                if (string.IsNullOrWhiteSpace(query))
                    return GetAllContacts();

                query = query.ToLower();
                return db.Contacts
                    .Where(c => c.FirstName.ToLower().Contains(query) ||
                                c.LastName.ToLower().Contains(query) ||
                                c.Phone.Contains(query) ||
                                c.Email.ToLower().Contains(query))
                    .ToList();
            }
        }

        public List<Contact> GetByCategory(string category)
        {
            using (var db = new ContactDbContext())
            {
                return db.Contacts
                    .Where(c => c.Category == category)
                    .ToList();
            }
        }

        public void ExportToJson(string filePath)
        {
            var contacts = GetAllContacts();
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var jsonString = JsonSerializer.Serialize(contacts, options);
            File.WriteAllText(filePath, jsonString);
        }

        public void ImportFromJson(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var jsonString = File.ReadAllText(filePath);
            var contacts = JsonSerializer.Deserialize<List<Contact>>(jsonString);

            if (contacts != null && contacts.Count > 0)
            {
                using (var db = new ContactDbContext())
                {
                    db.Contacts.AddRange(contacts);
                    db.SaveChanges();
                }
            }
        }
    }
}