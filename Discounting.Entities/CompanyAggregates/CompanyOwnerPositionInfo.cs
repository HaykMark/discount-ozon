using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class CompanyOwnerPositionInfo : ICompanyAggregate
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string LastName { get; set; }
        public string Citizenship { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime AuthorityValidityDate { get; set; }
        public string IdentityDocument { get; set; }
        public bool IsResident { get; set; }
    }
    
    public class CompanyPositionInfoConfiguration : IEntityTypeConfiguration<CompanyOwnerPositionInfo>
    {
        public void Configure(EntityTypeBuilder<CompanyOwnerPositionInfo> builder)
        {
            builder.ToTable("CompanyPositionInfos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Number).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.FirstName).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.SecondName).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.LastName).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Citizenship).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.PlaceOfBirth).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.IdentityDocument).HasColumnType("varchar(500)").IsRequired();

            builder
                .HasOne(c => c.Company)
                .WithOne(c => c.CompanyOwnerPositionInfo)
                .HasForeignKey<CompanyOwnerPositionInfo>(c => c.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}