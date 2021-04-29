using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.TariffDiscounting
{
    public class TariffArchive : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public decimal FromAmount { get; set; }
        public decimal? UntilAmount { get; set; }
        public int FromDay { get; set; }
        public int? UntilDay { get; set; }
        public decimal Rate { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime ActionTime { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }

        public User Creator { get; set; }
        public User User { get; set; }
    }
    
    public class TariffArchiveConfiguration : IEntityTypeConfiguration<TariffArchive>
    {
        public void Configure(EntityTypeBuilder<TariffArchive> builder)
        {
            builder.ToTable("TariffArchives");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.FromAmount).HasColumnType("numeric(18,2)");
            builder.Property(e => e.UntilAmount).HasColumnType("numeric(18,2)");
            builder.Property(e => e.Rate).HasColumnType("numeric(6,3)");
            
            builder.HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired();
            
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        }
    }
}