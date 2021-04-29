using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class Company : IEntity<Guid>
    {
        public Company()
        {
            FactoringAgreements = new List<FactoringAgreement>();
            CompanyBankInfos = new List<CompanyBankInfo>();
            Users = new HashSet<User>();
        }
        
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string TIN { get; set; }
        public string KPP { get; set; }
        public string PSRN { get; set; }
        public string IncorporationForm { get; set; }
        public string RegisteringAuthorityName { get; set; }
        public string RegistrationStatePlace { get; set; }
        public string StateStatisticsCode { get; set; }
        public string PaidUpAuthorizedCapitalInformation { get; set; }
        public DateTime? StateRegistrationDate { get; set; }
        public string OwnerFullName { get; set; }
        public string OwnerPosition { get; set; }
        public string OwnerDocument { get; set; }
        public bool IsActive { get; set; }
        public bool HasPowerOfAttorney { get; set; }
        public string DeactivationReason { get; set; }
        
        public CompanyType CompanyType { get; set; }
        public ICollection<User> Users { get; set; }
        public List<FactoringAgreement> FactoringAgreements { get; }
        public CompanySettings CompanySettings { get; set; }
        public CompanyAuthorizedUserInfo CompanyAuthorizedUserInfo { get; set; }
        public CompanyContactInfo CompanyContactInfo { get; set; }
        public CompanyOwnerPositionInfo CompanyOwnerPositionInfo { get; set; }
        public ICollection<CompanyBankInfo> CompanyBankInfos { get; set; }
        public ICollection<MigrationCardInfo> MigrationCardInfos { get; set; }
        public ICollection<ResidentPassportInfo> ResidentPassportInfos { get; set; }
    }

    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.TIN).IsUnique();
            builder.Property(e => e.FullName).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.IncorporationForm).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.ShortName).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.DeactivationReason).HasColumnType("varchar(2000)").IsRequired(false);
            builder.Property(e => e.PSRN).HasColumnType("varchar(15)").IsRequired(false);
            builder.Property(e => e.TIN).HasColumnType("varchar(12)").IsRequired();
            
            builder.Property(e => e.RegisteringAuthorityName).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.StateStatisticsCode).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.RegistrationStatePlace).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.PaidUpAuthorizedCapitalInformation).HasColumnType("varchar(500)").IsRequired(false);
            
            builder.Property(e => e.KPP).HasColumnType("varchar(12)").IsRequired(false);
            builder.Property(e => e.OwnerFullName).HasColumnType("varchar(300)").IsRequired(false);
            builder.Property(e => e.OwnerPosition).HasColumnType("varchar(300)").IsRequired(false);
            builder.Property(e => e.OwnerDocument).HasColumnType("varchar(250)").IsRequired(false);
        }
    }
    
    public enum CompanyType : byte
    {
        SellerBuyer = 1,
        Bank = 2
    }
}