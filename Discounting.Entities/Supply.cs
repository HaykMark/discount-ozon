using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.TariffDiscounting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class Supply : IEntity<Guid>
    {
        public Supply()
        {
            ChildSupplies = new List<Supply>();
        }
        public Guid Id { get; set; }
        public string Number { get; set; }
        public DateTime Date { get; set; }
        public SupplyType Type { get; set; }
        public decimal Amount{ get; set; }
        public Guid ContractId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid? BaseDocumentId { get; set; }
        public string BaseDocumentNumber { get; set; }
        public SupplyType BaseDocumentType { get; set; }
        public DateTime? BaseDocumentDate { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime DelayEndDate { get; set; }
        public DateTime CreationDate { get; set; }
        public Guid SupplyId { get; set; }
        public Guid? RegistryId { get; set; }
        public bool IsAccepted { get; set; }
        public SupplyProvider Provider { get; set; }
        public SupplyStatus Status { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool HasVerification { get; set; }
        public Guid? BankId { get; set; }
        public Guid? FactoringAgreementId { get; set; }
        public bool AddedBySeller { get; set; }
        public bool SellerVerified { get; set; }
        public bool BuyerVerified { get; set; }

        public Company Bank { get; set; }
        public Contract Contract { get; set; }
        public User Creator { get; set; }
        public FactoringAgreement FactoringAgreement  { get; set; }
        public SupplyDiscount SupplyDiscount { get; set; }
        public Registry Registry { get; set; }
        public List<Supply> ChildSupplies { get; set; }
        
        public static bool IsMainType(SupplyType type)
        {
            return type == SupplyType.Akt || type == SupplyType.Torg12 || type == SupplyType.Upd;
        }
    }

    public class SupplyConfiguration : IEntityTypeConfiguration<Supply>
    {
        public void Configure(EntityTypeBuilder<Supply> builder)
        {
            builder.ToTable("Supplies");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Amount).HasColumnType("numeric(18,2)");
            builder.Property(e => e.Number).HasColumnType("varchar(150)").IsRequired();
            builder.Property(e => e.ContractNumber).HasColumnType("varchar(150)").IsRequired();
            builder.Property(e => e.BaseDocumentNumber).HasColumnType("varchar(150)").IsRequired(false);
           
            builder.HasOne(e => e.Contract)
                .WithMany(e => e.Supplies)
                .HasForeignKey(e => e.ContractId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            
            builder.HasOne(e => e.Registry)
                .WithMany(e => e.Supplies)
                .HasForeignKey(e => e.RegistryId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            builder.HasOne(e => e.FactoringAgreement)
                .WithMany()
                .HasForeignKey(e => e.FactoringAgreementId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            builder.HasOne(e => e.Bank)
                .WithMany()
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
            
            builder.HasMany(b => b.ChildSupplies)
                .WithOne()
                .HasForeignKey(e => e.BaseDocumentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }

    public enum SupplyStatus : byte
    {
        InProcess = 1,
        InFinance = 2,
        NotAvailable = 3
    }
    public enum SupplyProvider : byte
    {
        Manually = 1,
        FromApi = 2,
        FromFtp = 3
    }

    public enum SupplyType : byte
    {
        None = 0,
        Torg12 = 1,
        Upd = 2,
        Akt = 3,
        Invoice = 4,
        Ukd = 5
    }
}