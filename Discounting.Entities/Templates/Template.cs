using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Templates
{
    public class Template: IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public TemplateType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        public CompanyAggregates.Company Company { get; set; }
    }
    
    public class TemplateConfiguration : IEntityTypeConfiguration<Template>
    {
        public void Configure(EntityTypeBuilder<Template> builder)
        {
            builder.ToTable("Templates");
            builder.HasKey(e => e.Id);
            builder.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.Property(e => e.Name).HasColumnType("varchar(1024)").IsRequired();
            builder.Property(e => e.Size).HasColumnType("bigint").IsRequired();
            builder.Property(e => e.ContentType).HasColumnType("varchar(255)").IsRequired();
        }
    }

    public enum TemplateType : byte
    {
        Registry = 1,
        Verification = 2,
        Discount = 3,
        ProfileRegulationSellerBuyer = 4, //Anketa 
        ProfileRegulationPrivateCompany = 5, //Anketa PC 
        ProfileRegulationBank = 6, //Anketa PC 
        ProfileRegulationUser = 7 //For new Users
    }
}