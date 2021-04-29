using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class SupplyFactoringAgreement : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid FactoringAgreementId { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public SupplyFactoringAgreementStatus Status { get; set; }
        
        public FactoringAgreement FactoringAgreement { get; set; }
    }
    
    public class SupplyFactoringAgreementConfiguration : IEntityTypeConfiguration<SupplyFactoringAgreement>
    {
        public void Configure(EntityTypeBuilder<SupplyFactoringAgreement> builder)
        {
            builder.ToTable("SupplyFactoringAgreements");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Number).HasColumnType("varchar(150)");

            builder.HasOne(e => e.FactoringAgreement)
                .WithMany(e => e.SupplyFactoringAgreements)
                .HasForeignKey(e => e.FactoringAgreementId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public enum SupplyFactoringAgreementStatus : byte
    {
        NotActive = 0,
        Active = 1,
        Blocked = 2
    }
}