using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class Contract : IEntity<Guid>
    {
        public Contract()
        {
            Supplies = new HashSet<Supply>();
        }

        public Guid Id { get; set; }
        public Guid SellerId { get; set; }
        public Guid BuyerId { get; set; }
        public ContractStatus Status { get; set; }
        public Guid? CreatorId { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public ContractProvider Provider { get; set; }
        public bool IsDynamicDiscounting { get; set; }
        public bool IsFactoring { get; set; }
        public bool IsRequiredRegistry { get; set; }
        public bool IsRequiredNotification { get; set; }
        public CompanyAggregates.Company Seller { get; set; }
        public CompanyAggregates.Company Buyer { get; set; }
        public User Creator { get; set; }
        public ICollection<Supply> Supplies { get; set; }
    }

    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contracts");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => new { e.SellerId, e.BuyerId }).IsUnique();
            builder.HasOne(e => e.Seller)
                .WithMany()
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Buyer)
                .WithMany()
                .HasForeignKey(e => e.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        }
    }

    public enum ContractStatus : byte
    {
        Active = 1,
        Blocked = 2,
        NotActive = 3
    }

    public enum ContractProvider : byte
    {
        Manually = 1,
        Automatically = 2
    }
}