using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Regulations
{
    public class UserProfileRegulationInfo : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public string Citizenship { get; set; }
        public string PlaceOfBirth { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? AuthorityValidityDate { get; set; }
        public string IdentityDocument { get; set; }
        public bool IsResident { get; set; }

        //For residents
        public string PassportSeries { get; set; }
        public DateTime? PassportDate { get; set; }
        public string PassportNumber { get; set; }
        public string PassportUnitCode { get; set; }
        public string PassportIssuingAuthorityPSRN { get; set; }
        public string PassportSNILS { get; set; }

        //For non residents
        public string MigrationCardRightToResideDocument { get; set; }
        public string MigrationCardAddress { get; set; }
        public string MigrationCardRegistrationAddress { get; set; }
        public string MigrationCardPhone { get; set; }
        public Guid UserRegulationId { get; set; }
        public UserRegulation UserRegulation { get; set; }
    }

    public class UserProfileRegulationInfoConfiguration : IEntityTypeConfiguration<UserProfileRegulationInfo>
    {
        public void Configure(EntityTypeBuilder<UserProfileRegulationInfo> builder)
        {
            builder.ToTable("UserProfileRegulationInfos");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Number).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.Citizenship).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.PlaceOfBirth).HasColumnType("varchar(500)").IsRequired();
            builder.Property(e => e.IdentityDocument).HasColumnType("varchar(500)").IsRequired();
            
            builder
                .HasOne(c => c.UserRegulation)
                .WithOne(c => c.UserProfileRegulationInfo)
                .HasForeignKey<UserProfileRegulationInfo>(c => c.UserRegulationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}