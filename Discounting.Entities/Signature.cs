using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public abstract class Signature : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid SignerId { get; set; }
        public SignatureType Type { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }

        public User Signer { get; set; }
        public SignatureInfo SignatureInfo { get; set; }
    }

    public class RegistrySignature : Signature
    {
        public Guid RegistryId { get; set; }
        public Registry Registry { get; set; }
    }
    
    public class UnformalizedDocumentSignature : Signature
    {
        public Guid UnformalizedDocumentId { get; set; }
        public UnformalizedDocument UnformalizedDocument { get; set; }
    }
    
    public class UploadSignature : Signature
    {
        public Guid UploadId { get; set; }
        public Upload Upload { get; set; }
    }
    
    public class CompanyRegulationSignature : Signature
    {
        public Guid CompanyRegulationId { get; set; }
        public CompanyRegulation CompanyRegulation { get; set; }
    }
    
    public class UserRegulationSignature : Signature
    {
        public Guid UserRegulationId { get; set; }
        public UserRegulation UserRegulation { get; set; }
    }

    public class SignatureConfiguration : IEntityTypeConfiguration<Signature>
    {
        public void Configure(EntityTypeBuilder<Signature> builder)
        {
            builder.ToTable("Signatures")
                .HasDiscriminator(c => c.Type)
                .HasValue<RegistrySignature>(SignatureType.Registry)
                .HasValue<CompanyRegulationSignature>(SignatureType.CompanyRegulation)
                .HasValue<UnformalizedDocumentSignature>(SignatureType.UnformalizedDocument)
                .HasValue<UserRegulationSignature>(SignatureType.UserRegulation)
                .HasValue<UploadSignature>(SignatureType.Upload);
            builder.HasKey(e => e.Id);
            builder.HasOne(b => b.Signer)
                .WithMany()
                .HasForeignKey(b => b.SignerId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public enum SignatureType : byte
    {
        Registry = 1,
        UnformalizedDocument = 2,
        Upload = 3,
        CompanyRegulation = 4,
        UserRegulation = 5
    }
}