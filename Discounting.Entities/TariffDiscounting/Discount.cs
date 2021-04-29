using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.TariffDiscounting
{
    public class Discount : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public DateTime PlannedPaymentDate { get; set; }
        public decimal AmountToPay { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal Rate { get; set; }
        public DiscountingSource DiscountingSource { get; set; }
        public bool HasChanged { get; set; }
        public Guid RegistryId { get; set; }
        public Registry Registry { get; set; }
    }
    
    public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.ToTable("Discounts");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Rate).HasColumnType("numeric(6,3)");
            builder.Property(e => e.AmountToPay).HasColumnType("numeric(18,2)");
            builder.Property(e => e.DiscountedAmount).HasColumnType("numeric(18,2)");
            
            builder.HasOne(d => d.Registry)
                .WithOne(p => p.Discount)
                .HasForeignKey<Discount>(d => d.RegistryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public enum DiscountingSource : byte
    {
        Personal = 1,
        Bank = 2
    }
}