using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Regulations
{
    public class UserRegulation : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public UserRegulationType Type { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public UserProfileRegulationInfo UserProfileRegulationInfo { get; set; }
    }

    public class UserRegulationConfiguration : IEntityTypeConfiguration<UserRegulation>
    {
        public void Configure(EntityTypeBuilder<UserRegulation> builder)
        {
            builder.ToTable("UserRegulations");
            builder.HasKey(e => e.Id);

            builder
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public enum UserRegulationType : byte
    {
        Profile = 1,
        Other = 2
    }
}