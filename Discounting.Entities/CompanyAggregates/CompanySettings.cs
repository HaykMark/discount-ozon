using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class CompanySettings : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid? DefaultTariff { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsSendAutomatically { get; set; }
        public bool IsAuction { get; set; }
        public bool ForbidSellerEditTariff { get; set; }

        public Company Company { get; set; }
        public User User { get; set; }
    }

    public class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
    {
        public void Configure(EntityTypeBuilder<CompanySettings> builder)
        {
            builder.ToTable("CompanySettings");
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.Company)
                .WithOne(e => e.CompanySettings)
                .HasForeignKey<CompanySettings>(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<CompanySettings>(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}