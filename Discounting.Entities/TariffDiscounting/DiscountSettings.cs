using System;
using Discounting.Common.Types;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.TariffDiscounting
{
    public class DiscountSettings : IEntity<Guid>, IMeta
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public int MinimumDaysToShift { get; set; }
        public DaysType DaysType { get; set; }
        public DaysOfWeek PaymentWeekDays { get; set; }
        public Company Company { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
    
    public class DiscountSettingsConfiguration : IEntityTypeConfiguration<DiscountSettings>
    {
        public void Configure(EntityTypeBuilder<DiscountSettings> builder)
        {
            builder.ToTable("DiscountSettings");
            builder.HasKey(e => e.Id);
            
            builder.HasOne(e => e.Company)
                .WithOne()
                .HasForeignKey<DiscountSettings>(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        }
    }

    public enum DaysType : byte
    {
        None = 0,
        Business = 1,
        Calendar = 2
    }

    [Flags]
    public enum DaysOfWeek
    {
        None = 0,
        Sunday = 1 << 0,
        Monday = 1 << 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 3,
        Thursday = 1 << 4,
        Friday = 1 << 5,
        Saturday = 1 << 6,
    }
}