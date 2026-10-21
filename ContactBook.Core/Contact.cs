using System;
namespace ContactBook.Core
{
    public class Contact
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Category { get; set; } = "Общие";
        public DateTime? BirthDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string FullName => $"{LastName} {MiddleName}".Trim();
    }
}