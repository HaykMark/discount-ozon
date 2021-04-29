using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class FactoringAgreement : IEntity<Guid>
    {
        public FactoringAgreement()
        {
            SupplyFactoringAgreements = new List<SupplyFactoringAgreement>();
        }
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid BankId { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsActive { get; set; }

        public string FactoringContractNumber { get; set; }
        public DateTime? FactoringContractDate { get; set; }
        
        //Bank info
        public string BankName { get; set; }
        public string BankCity { get; set; }
        public string BankBic { get; set; }
        public string BankOGRN { get; set; }
        public string BankCorrespondentAccount { get; set; }
        public string BankCheckingAccount { get; set; }

        public Company Company { get; set; }
        public Company Bank { get; set; }
        public List<SupplyFactoringAgreement> SupplyFactoringAgreements { get; set; }
    }

    public class FactoringAgreementConfiguration : IEntityTypeConfiguration<FactoringAgreement>
    {
        public void Configure(EntityTypeBuilder<FactoringAgreement> builder)
        {
            builder.ToTable("FactoringAgreements");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FactoringContractNumber).HasColumnType("varchar(150)");
            builder.Property(e => e.BankName).HasColumnType("varchar(250)").IsRequired(false);
            builder.Property(e => e.BankCity).HasColumnType("varchar(250)").IsRequired(false);
            builder.Property(e => e.BankBic).HasColumnType("varchar(9)").IsRequired(false);
            builder.Property(e => e.BankOGRN).HasColumnType("varchar(13)").IsRequired(false);
            builder.Property(e => e.BankCorrespondentAccount).HasColumnType("varchar(20)").IsRequired(false);
            builder.Property(e => e.BankCheckingAccount).HasColumnType("varchar(20)").IsRequired(false);

            builder.HasOne(e => e.Company)
                .WithMany(e => e.FactoringAgreements)
                .HasForeignKey(e => e.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.Bank)
                .WithMany()
                .HasForeignKey(e => e.BankId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}