using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Auditing
{
    public class Audit : IEntity<int>
    {
        public int Id { get; set; }
        public IncidentType Incident { get; set; }
        public string SourceId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime IncidentDate { get; set; }
        public string IpAddress { get; set; }
        public string Message { get; set; }
        public IncidentResult IncidentResult  { get; set; }

        public User User { get; set; }
    }

    public class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.ToTable("Audits");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.SourceId).HasColumnType("varchar(500)").IsRequired(false);
            builder.Property(e => e.IpAddress).HasColumnType("varchar(50)").IsRequired(false);
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public enum IncidentResult : byte
    {
        None = 0,
        Success = 1,
        Failed = 2
    }

    public enum IncidentType
    {
        //User
        UserRegistered,
        UserAdded,
        UserEmailConfirmed,
        CompanyRegulationUploaded,
        CompanyRegulationCreated,
        CompanyRegulationUpdated,
        CompanyRegulationSigned,
        NewUserRegulationCreated,
        NewUserRegulationUploaded,
        NewUserRegulationSigned,
        UserBlocked,
        UserUnblocked,
        UserLoggedIn,
        UserLoggedOut,
        
        //Settings
        ContractCreated,
        ContractUpdated,
        CompanySettingsCreated,
        CompanySettingsUpdated,
        TariffCreated,
        TemplatedAddedToBuyer,
        TemplateForBuyerUpdated,
        
        //Supllies
        SuppliesAdded,
        SuppliesVerifiedSeller,
        SuppliesVerifiedBuyer,

        //Discount
        DiscountCreated,
        DiscountRegistryConfirmed,
        DiscountConfirmedPercentageChanged,
        DiscountRegistryUpdated,
        DiscountRegistryDeclined,
        
        //Registry
        RegistryCreated,
        RegistryUpdated,
        RegistrySigned,
        RegistryDeclined,
        
        //Unformalized document
        UFDocumentCreated,
        UFDocumentSigned,
        UFDocumentDeclined,
        
        PasswordChanged,
        PasswordReset,
        
        RegistrySignatureVerification,
        UFDocumentSignatureVerification,
        CompanyRegulationSignatureVerification,
        UserRegulationSignatureVerification
    }
}