using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.Regulations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Account
{
    public class User : IEntity<Guid>, IActivatable
    {
        public User()
        {
            UserRoles = new HashSet<UserRole>();
            UserTokens = new HashSet<UserToken>();
        }

        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string SecondName { get; set; }
        public string Position { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public string Salt { get; set; }

        public string DisplayName { get; set; }
        public bool CanSign { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool IsTestUser { get; set; }
        public DateTime CreationDate { get; set; }

        public string ActivationToken { get; set; }

        public DateTime? ActivationTokenCreationDateTime { get; set; }

        public DateTimeOffset? LastLoggedIn { get; set; }

        /// <summary>
        /// every time the user changes his Password,
        /// or an admin changes his Roles or state/IsActive,
        /// create a new `SerialNumber` GUID and store it in the DB.
        /// </summary>
        public string SerialNumber { get; set; }

        public string Email { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsConfirmedByAdmin { get; set; }
        public string DeactivationReason { get; set; }
        public int PasswordRetryLimit { get; set; }
        public Guid CompanyId { get; set; }

        public CompanyAggregates.Company Company { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

        public ICollection<UserToken> UserTokens { get; set; }
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Email).IsUnique();
            builder.Property(e => e.FirstName).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Surname).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.SecondName).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.Phone).HasColumnType("varchar(11)").IsRequired(false);
            builder.Property(e => e.IsActive).IsRequired();
            builder.Property(e => e.Position).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.ActivationToken).HasColumnType("varchar(4000)").IsRequired(false);
            builder.Property(e => e.DeactivationReason).HasColumnType("varchar(2000)").IsRequired(false);
            builder.Property(e => e.SerialNumber).HasColumnType("varchar(50)").IsRequired();
            builder.Property(e => e.Email).HasColumnType("varchar(450)").IsRequired();
            builder.Property(e => e.Password).HasColumnType("varchar(4000)").IsRequired();
            builder.Property(e => e.Salt).HasColumnType("varchar(2000)").IsRequired();
            builder.Property(e => e.CreationDate).IsRequired();
            builder.Property(e => e.IsAdmin).HasDefaultValue(false);
            builder.Property(e => e.IsTestUser).HasDefaultValue(false);
            builder.HasOne(e => e.Company)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}