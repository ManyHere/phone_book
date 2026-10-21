using Microsoft.EntityFrameworkCore;
using ContactBook.Core.Models;

namespace ContactBook.Data
{
    public class ContactDbContext : DbContext
    {
        public DbSet<Contact> Contacts => Set<Contact>();
        public DbSet<Group> Groups => Set<Group>();

        public ContactDbContext()
        {
        }

        public ContactDbContext(DbContextOptions<ContactDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=contacts.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).HasMaxLength(100);
                entity.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Email).HasMaxLength(150);
                entity.Property(c => c.Notes).HasMaxLength(1000);

                entity.HasOne(c => c.Group)
                      .WithMany()
                      .HasForeignKey(c => c.GroupId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.Property(g => g.Name).IsRequired().HasMaxLength(50);

                entity.HasData(
                    new Group { Id = 1, Name = "Общие" },
                    new Group { Id = 2, Name = "Работа" },
                    new Group { Id = 3, Name = "Семья" },
                    new Group { Id = 4, Name = "Друзья" },
                    new Group { Id = 5, Name = "Избранное" }
                );
            });
        }
    }
}
