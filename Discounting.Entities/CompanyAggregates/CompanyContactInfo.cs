using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class CompanyContactInfo : ICompanyAggregate
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public string Address { get; set; }
        public string OrganizationAddress { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string MailingAddress { get; set; }
        public string NameOfGoverningBodies { get; set; }
    }
    
    public class CompanyContactInfoConfiguration : IEntityTypeConfiguration<CompanyContactInfo>
    {
        public void Configure(EntityTypeBuilder<CompanyContactInfo> builder)
        {
            builder.ToTable("CompanyContactInfos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Address).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Phone).HasColumnType("varchar(11)").IsRequired();
            builder.Property(e => e.OrganizationAddress).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Email).HasColumnType("varchar(50)").IsRequired();
            builder.Property(e => e.MailingAddress).HasColumnType("varchar(50)").IsRequired(false);
            builder.Property(e => e.NameOfGoverningBodies).HasColumnType("varchar(500)").IsRequired();

            builder
                .HasOne(c => c.Company)
                .WithOne(c => c.CompanyContactInfo)
                .HasForeignKey<CompanyContactInfo>(c => c.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}