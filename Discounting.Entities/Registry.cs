using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Discounting.Entities.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class Registry : IEntity<Guid>
    {
        public Registry()
        {
            Supplies = new HashSet<Supply>();
        }

        public Guid Id { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public Guid ContractId { get; set; }
        public Guid CreatorId { get; set; }
        public RegistryStatus Status { get; set; }
        public RegistrySignStatus SignStatus { get; set; }
        public FinanceType FinanceType { get; set; }
        public bool IsVerified { get; set; }
        public bool IsConfirmed { get; set; }
        public string Remark { get; set; }
        public Guid? BankId { get; set; }
        public Guid? FactoringAgreementId { get; set; }

        public Contract Contract { get; set; }
        public Company Bank { get; set; }
        public Company Creator { get; set; }
        public Discount Discount { get; set; }
        public FactoringAgreement FactoringAgreement { get; set; }

        public ICollection<Supply> Supplies { get; set; }

        public string GetFileName(TemplateType type)
        {
            var infix = $"_{Number}" +
                        $"_{Date:dd.MM.yyyy}" +
                        $"_{Contract.Seller.TIN}" +
                        $"_{Contract.Buyer.TIN}";
            return type switch
            {
                TemplateType.Registry => $"Registry" +
                                         infix +
                                         $"_{Bank.TIN}" +
                                         $".xlsx",
                TemplateType.Verification => $"Verification" +
                                             infix +
                                             $"_{Bank.TIN}" +
                                             $".xlsx",
                TemplateType.Discount => $"Discount" +
                                         infix +
                                         $".xlsx",
                _ => ""
            };
        }
    }

    public class RegistryConfiguration : IEntityTypeConfiguration<Registry>
    {
        public void Configure(EntityTypeBuilder<Registry> builder)
        {
            builder.ToTable("Registries");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Number).IsRequired();
            builder.Property(e => e.Remark).HasColumnType("varchar(4000)");
            builder.Property(e => e.Status).HasDefaultValue(RegistryStatus.InProcess).IsRequired();
            builder.Property(e => e.SignStatus).HasDefaultValue(RegistrySignStatus.NotSigned).IsRequired();

            builder.HasOne(e => e.Contract)
                .WithMany()
                .HasForeignKey(e => e.ContractId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(e => e.Bank)
                .WithMany()
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
            
            builder.HasOne(e => e.FactoringAgreement)
                .WithMany()
                .HasForeignKey(e => e.FactoringAgreementId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }

    public enum RegistryStatus : byte
    {
        InProcess = 1,
        Finished = 2,
        Declined = 3
    }

    public enum RegistrySignStatus : byte
    {
        NotSigned = 1,
        SignedBySeller = 2,
        SignedByBuyer = 3,
        SignedBySellerBuyer = 4,
        SignedByAll = 5
    }
    
    public enum FinanceType : byte
    {
        None = 0,
        DynamicDiscounting = 1,
        SupplyVerification = 2
    }
}