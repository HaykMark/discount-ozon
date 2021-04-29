using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.TariffDiscounting
{
    public class Tariff : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public decimal FromAmount { get; set; }
        public decimal? UntilAmount { get; set; }
        public int FromDay { get; set; }
        public int? UntilDay { get; set; }
        public decimal Rate { get; set; }
        public TariffType Type { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreationDate { get; set; }
        public User User { get; set; }
    }

    public class TariffConfiguration : IEntityTypeConfiguration<Tariff>
    {
        public void Configure(EntityTypeBuilder<Tariff> builder)
        {
            builder.ToTable("Tariffs");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FromAmount).HasColumnType("numeric(18,2)");
            builder.Property(e => e.UntilAmount).HasColumnType("numeric(18,2)");
            builder.Property(e => e.Rate).HasColumnType("numeric(6,3)");
            
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }

    public enum TariffType : byte
    {
        Discounting = 1,
        Yearly = 2
    }
}