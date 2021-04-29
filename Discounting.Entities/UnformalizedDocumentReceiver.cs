using System;
using Discounting.Common.Types;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class UnformalizedDocumentReceiver : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid UnformalizedDocumentId { get; set; }
        public Guid ReceiverId { get; set; }
        public bool NeedSignature { get; set; }
        public bool IsSigned { get; set; }

        public UnformalizedDocument UnformalizedDocument { get; set; }
        public Company Receiver { get; set; }
    }
    
    public class UnformalizedDocumentReceiverConfiguration : IEntityTypeConfiguration<UnformalizedDocumentReceiver>
    {
        public void Configure(EntityTypeBuilder<UnformalizedDocumentReceiver> builder)
        {
            builder.ToTable("UnformalizedDocumentReceivers");
            builder.HasKey(e => e.Id);

            builder.HasOne(e => e.UnformalizedDocument)
                .WithMany(e => e.Receivers)
                .HasForeignKey(e => e.UnformalizedDocumentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Receiver)
                .WithMany()
                .HasForeignKey(e => e.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
        }
    }
}