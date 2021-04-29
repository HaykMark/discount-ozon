using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Account
{
    public class UserToken : IEntity<int>
    {
        public int Id { get; set; }
        public string AccessTokenHash { get; set; }
        public DateTimeOffset AccessTokenExpiresDateTime { get; set; }
        public string RefreshTokenIdHash { get; set; }
        public string RefreshTokenIdHashSource { get; set; }
        public DateTimeOffset RefreshTokenExpiresDateTime { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }

    public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
    {
        public void Configure(EntityTypeBuilder<UserToken> builder)
        {
            builder.ToTable("UserTokens");
            builder.HasOne(ut => ut.User)
                .WithMany(u => u.UserTokens)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(ut => ut.RefreshTokenIdHash).HasColumnType("varchar(450)").IsRequired();
            builder.Property(ut => ut.RefreshTokenIdHashSource).HasColumnType("varchar(450)");
        }
    }
}