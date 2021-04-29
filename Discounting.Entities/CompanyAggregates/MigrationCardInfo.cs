using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class MigrationCardInfo : ICompanyAggregate
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
        public string RightToResideDocument { get; set; }
        public string Address { get; set; }
        public string RegistrationAddress { get; set; }
        public string Phone { get; set; }
        public CompanyPositionType PositionType { get; set; }
    }
    
    public class MigrationCardDataInfoConfiguration : IEntityTypeConfiguration<MigrationCardInfo>
    {
        public void Configure(EntityTypeBuilder<MigrationCardInfo> builder)
        {
            builder.ToTable("MigrationCardInfos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.RightToResideDocument).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Address).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.RegistrationAddress).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.Phone).HasColumnType("varchar(11)").IsRequired();

            builder
                .HasOne(c => c.Company)
                .WithMany(c => c.MigrationCardInfos)
                .HasForeignKey(c => c.CompanyId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}