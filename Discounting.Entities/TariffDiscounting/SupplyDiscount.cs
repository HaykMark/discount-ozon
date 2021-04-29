using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.TariffDiscounting
{
    public class SupplyDiscount : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountedAmount { get; set; }
        public Guid SupplyId { get; set; }
        public Supply Supply { get; set; }
    }
    
    public class SupplyDiscountConfiguration : IEntityTypeConfiguration<SupplyDiscount>
    {
        public void Configure(EntityTypeBuilder<SupplyDiscount> builder)
        {
            builder.ToTable("SupplyDiscounts");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Rate).HasColumnType("numeric(6,3)");
            builder.Property(e => e.DiscountedAmount).HasColumnType("numeric(18,2)");
            
            builder.HasOne(d => d.Supply)
                .WithOne(p => p.SupplyDiscount)
                .HasForeignKey<SupplyDiscount>(d => d.SupplyId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}