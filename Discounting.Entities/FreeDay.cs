using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities
{
    public class FreeDay : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }

    public class CalendarConfiguration : IEntityTypeConfiguration<FreeDay>
    {
        public void Configure(EntityTypeBuilder<FreeDay> builder)
        {
            builder.ToTable("FreeDays");
            builder.HasKey(e => e.Id);
        }
    }
}