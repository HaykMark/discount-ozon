using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class CompanyBankInfo : ICompanyAggregate, IActivatable
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string Bic { get; set; }
        public string OGRN { get; set; }
        public string CorrespondentAccount { get; set; }
        public string CheckingAccount { get; set; }
        public bool IsActive { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Company Company { get; set; }
    }
    
    public class CompanyBankInfoConfiguration : IEntityTypeConfiguration<CompanyBankInfo>
    {
        public void Configure(EntityTypeBuilder<CompanyBankInfo> builder)
        {
            builder.ToTable("CompanyBankInfos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).HasColumnType("varchar(250)").IsRequired();
            builder.Property(e => e.City).HasColumnType("varchar(250)").IsRequired();
            builder.Property(e => e.Bic).HasColumnType("varchar(9)").IsRequired();
            builder.Property(e => e.OGRN).HasColumnType("varchar(13)").IsRequired();
            builder.Property(e => e.CorrespondentAccount).HasColumnType("varchar(20)").IsRequired();
            builder.Property(e => e.CheckingAccount).HasColumnType("varchar(20)").IsRequired();
            
            builder
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.ClientNoAction);

            builder
                .HasOne(c => c.Company)
                .WithMany(c => c.CompanyBankInfos)
                .HasForeignKey(c => c.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}