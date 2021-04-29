using System;
using Discounting.Common.AccessControl;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Account
{
    public class Permission
    {
        public int Id { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; }
        public string ZoneId { get; set; }
        public Operations Operations { get; set; }
    }

    public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        /// <remarks>
        /// Note: seeding of permissions depends on zones, which are generated
        /// in the Web project, hence seeding is done in Web.
        /// </remarks>
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(e => e.Id);
            builder
                .HasOne(e => e.Role)
                .WithMany(r => r.Permissions)
                .HasForeignKey(nameof(Permission.RoleId))
                .IsRequired(true);
        }
    }
}