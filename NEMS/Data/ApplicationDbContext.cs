using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NEMS.Models;

namespace NEMS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());

            builder.Entity<TimeTable>().HasIndex(x => x.date);

            string ADMIN_ID = "51da857c-224d-40d3-9ffc-9fb03c9f6e8a";
            string ROLE_ID = "48ea8115-b808-40b3-8abd-d286e199cc42";

            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Name = "Admin",
                NormalizedName = "ADMIN",
                Id = ROLE_ID,
                ConcurrencyStamp = ROLE_ID
            }, new IdentityRole
            {
                Name = "Sales",
                NormalizedName = "SALES",
                Id = "2c6f3a66-652e-4fcc-b90a-184bd6463f28",
                ConcurrencyStamp = "2c6f3a66-652e-4fcc-b90a-184bd6463f28"
            }, new IdentityRole
            {
                Name = "User",
                NormalizedName = "USER",
                Id = "19687fdd-11a2-4dee-a409-fc8032e5ff93",
                ConcurrencyStamp = "19687fdd-11a2-4dee-a409-fc8032e5ff93"
            });

            var adminUser = new ApplicationUser
            {
                Id = ADMIN_ID,
                Email = "th_accounting@nc-net.or.jp",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "Admin",
                Title = "Admin",
                UserName = "th_accounting@nc-net.or.jp",
                NormalizedUserName = "TH_ACCOUNTING@NC-NET.OR.JP"
            };

            PasswordHasher<ApplicationUser> passwordHasher = new PasswordHasher<ApplicationUser>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Fna12345!");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = ROLE_ID,
                UserId = ADMIN_ID,
            });
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<TimeTable> TimeTables { get; set; }
        public DbSet<Event> Events { get; set; }
    }

    internal class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(255);
            builder.Property(u => u.LastName).HasMaxLength(255);
            builder.Property(u => u.Title).HasMaxLength(255);
        }
    }
}