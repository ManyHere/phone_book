using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactBook.Core.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Notes { get; set; }
        public byte[]? Photo { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
