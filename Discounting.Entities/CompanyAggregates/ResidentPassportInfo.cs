using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class ResidentPassportInfo : ICompanyAggregate
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public string Series { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string UnitCode { get; set; }
        public string IssuingAuthorityPSRN { get; set; }
        public string SNILS { get; set; }
        public CompanyPositionType PositionType { get; set; }
    }
    
    public class ResidentPassportInfoConfiguration : IEntityTypeConfiguration<ResidentPassportInfo>
    {
        public void Configure(EntityTypeBuilder<ResidentPassportInfo> builder)
        {
            builder.ToTable("ResidentPassportInfos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Series).HasColumnType("varchar(4)").IsRequired();
            builder.Property(e => e.Number).HasColumnType("varchar(6)").IsRequired();
            builder.Property(e => e.UnitCode).HasColumnType("varchar(7)").IsRequired();
            builder.Property(e => e.IssuingAuthorityPSRN).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.SNILS).HasColumnType("varchar(500)").IsRequired(false);

            builder
                .HasOne(c => c.Company)
                .WithMany(c => c.ResidentPassportInfos)
                .HasForeignKey(c => c.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}