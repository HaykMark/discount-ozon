using System;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    /// <summary>
    /// Upload entity type
    /// </summary>
    public class Upload : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public UploadProvider Provider { get; set; }
        public Guid ProviderId { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public User User { get; set; }
    }

    public class UploadConfiguration : IEntityTypeConfiguration<Upload>
    {
        public void Configure(EntityTypeBuilder<Upload> builder)
        {
            builder.ToTable("Uploads");
            builder.HasKey(e => e.Id);
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired();
            builder.Property(e => e.Name).HasColumnType("varchar(1024)").IsRequired();
            builder.Property(e => e.Size).HasColumnType("bigint").IsRequired();
            builder.Property(e => e.ContentType).HasColumnType("varchar(255)").IsRequired();
        }
    }

    public enum UploadProvider : byte
    {
        Supply = 1,
        Registry = 2,
        Regulation = 3,
        UnformalizedDocument = 4,
        UserRegulation = 5
    }
}