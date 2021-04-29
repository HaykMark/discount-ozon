using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class SignatureInfo : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Serial { get; set; }
        public string Thumbprint { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTill { get; set; }
        public string Company { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string INN { get; set; }
        public string OGRN { get; set; }
        public string SNILS { get; set; }
        public Guid SignatureId { get; set; }

        public Signature Signature { get; set; }
    }
    
    public class SignatureInfoConfiguration : IEntityTypeConfiguration<SignatureInfo>
    {
        public void Configure(EntityTypeBuilder<SignatureInfo> builder)
        {
            builder.ToTable("SignatureInfos");
                
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Serial).HasColumnType("varchar(1000)");
            builder.Property(e => e.Company).HasColumnType("varchar(1000)");
            builder.Property(e => e.Name).HasColumnType("varchar(1000)");
            builder.Property(e => e.Email).HasColumnType("varchar(400)");
            builder.Property(e => e.INN).HasColumnType("varchar(20)");
            builder.Property(e => e.OGRN).HasColumnType("varchar(20)");
            builder.Property(e => e.SNILS).HasColumnType("varchar(20)");
            builder.Property(e => e.Thumbprint).HasColumnType("varchar(2000)");

            builder.HasOne(b => b.Signature)
                .WithOne(s => s.SignatureInfo)
                .HasForeignKey<SignatureInfo>(b => b.SignatureId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}