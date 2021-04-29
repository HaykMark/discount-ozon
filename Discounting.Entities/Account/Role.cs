using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Account
{
    public class Role : IEntity<Guid>, IWithSystemFlag
    {
        public Role()
        {
            UserRoles = new HashSet<UserRole>();
        }

        public Guid Id { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
        public string Description { get; set; }
        public Permissions Permissions { get; set; }
        public string Name { get; set; }
        public string Remarks { get; set; }
        public RoleType Type { get; set; }
        public bool IsSystemDefault { get; set; }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Name).IsUnique();
            builder.Property(e => e.Description).HasColumnType("varchar(500)");
            builder.Property(e => e.Remarks).HasColumnType("varchar(500)");
        }
    }

    public enum RoleType : byte
    {
        Other = 0,
        SuperAdmin = 1,
        SellerBuyer = 2,
        Bank = 3,
        InactiveCompany = 4,
        NoPermission = 5
    }
}