using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Templates
{
    public class BuyerTemplateConnection : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid BuyerId { get; set; }
        public Guid BankId { get; set; }
        public Guid TemplateId { get; set; }

        public CompanyAggregates.Company Buyer { get; set; }
        public CompanyAggregates.Company Bank { get; set; }
        public Template Template { get; set; }
    }

    public class BuyerTemplateConnectionConfiguration : IEntityTypeConfiguration<BuyerTemplateConnection>
    {
        public void Configure(EntityTypeBuilder<BuyerTemplateConnection> builder)
        {
            builder.ToTable("BuyerTemplateConnections");
            builder.HasKey(e => e.Id);

            builder
                .HasOne(e => e.Buyer)
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            builder
                .HasOne(e => e.Bank)
                .WithMany()
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();

            builder.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}