using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.CompanyAggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class UnformalizedDocument : IEntity<Guid>
    {
        public UnformalizedDocument()
        {
            Receivers = new HashSet<UnformalizedDocumentReceiver>();
        }

        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public UnformalizedDocumentType Type { get; set; }
        public string Topic { get; set; }
        public string Message { get; set; }
        public UnformalizedDocumentStatus Status { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? SentDate { get; set; }
        public Guid? DeclinedBy { get; set; }
        public DateTime? DeclinedDate { get; set; }
        public string DeclineReason { get; set; }
        public Company Sender { get; set; }
        public Company Decliner { get; set; }
        public ICollection<UnformalizedDocumentReceiver> Receivers { get; set; }
    }

    public class UnformalizedDocumentConfiguration : IEntityTypeConfiguration<UnformalizedDocument>
    {
        public void Configure(EntityTypeBuilder<UnformalizedDocument> builder)
        {
            builder.ToTable("UnformalizedDocuments");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Topic).HasColumnType("varchar(100)").IsRequired();
            builder.Property(e => e.Message).HasColumnType("varchar(1000)");
            builder.Property(e => e.DeclineReason).HasColumnType("varchar(1000)");
            builder.Property(e => e.Status).HasDefaultValue(UnformalizedDocumentStatus.Draft).IsRequired();
            builder.Property(e => e.Type).HasDefaultValue(UnformalizedDocumentType.UnformalizedDocument).IsRequired();

            builder.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);
            builder.HasOne(e => e.Decliner)
                .WithMany()
                .HasForeignKey(e => e.DeclinedBy)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }

    public enum UnformalizedDocumentType : byte
    {
        Agreement = 1,
        SupplementaryAgreement = 2, // Дополнительное соглашение
        FactoringNotice = 3, // Уведомление о факторинге
        ReturnAssignmentNotice = 4, // Уведомление об обратной уступке
        UnformalizedDocument = 5 // Неформализованный документ
    }

    public enum UnformalizedDocumentStatus : byte
    {
        None = 0,
        Draft = 1,
        NeedReceiverSignature = 2,
        SignedByAll = 3,
        Declined = 4
    }
}