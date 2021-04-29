using System;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.Regulations
{
    public class Regulation : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public RegulationType Type { get; set; }
    }
    
    public class RegulationConfiguration : IEntityTypeConfiguration<Regulation>
    {
        public void Configure(EntityTypeBuilder<Regulation> builder)
        {
            builder.ToTable("Regulations");
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Type).IsUnique();
        }
    }

    public enum RegulationType : byte
    {
        PersonalDataProcessingPolicy = 1,
        ETPPrivacyPolicy = 2,
        ConditionsRegulationsPolicy = 3,
        FullFledgedWorkReadinessPolicy = 4,
        FactoringOffer = 5,
        DiscountingOffer = 6
    }
}