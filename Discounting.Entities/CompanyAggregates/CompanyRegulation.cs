using System;
using System.Collections.Generic;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Discounting.Entities.CompanyAggregates
{
    public class CompanyRegulation : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public CompanyRegulationType Type { get; set; }
        public DateTime CreatedDate { get; set; }
        public User User { get; set; }
        public Company Company { get; set; }

        public static HashSet<CompanyRegulationType> GetRequiredRegulations()
        {
            return new HashSet<CompanyRegulationType>
            {
                CompanyRegulationType.Profile,
                CompanyRegulationType.PSRNCertificate,
                CompanyRegulationType.TINCertificate,
                CompanyRegulationType.ConstituentDocumentsWithAmendmentsAndAddition,
                CompanyRegulationType.ElectionDocument
            };
        }
    }
    
    public class CompanyRegulationConfiguration : IEntityTypeConfiguration<CompanyRegulation>
    {
        public void Configure(EntityTypeBuilder<CompanyRegulation> builder)
        {
            builder.ToTable("CompanyRegulations");
            builder.HasKey(e => e.Id);
            builder.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .IsRequired();
            builder.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .IsRequired();

            builder.Property(e => e.Name).HasColumnType("varchar(1024)").IsRequired();
            builder.Property(e => e.Size).HasColumnType("bigint").IsRequired();
            builder.Property(e => e.ContentType).HasColumnType("varchar(255)").IsRequired();
        }
    }

    public enum CompanyRegulationType : byte
    {
        Profile = 1, //Анкета
        PSRNCertificate = 2, // Свидетельство ОГРН
        TINCertificate = 3, // Свидетельство ИНН
        ConstituentDocumentsWithAmendmentsAndAddition = 4, // Учредительные документы с изменениями и дополнениями
        ElectionDocument = 5, // Документ об избрании
        License = 6, //Лицензия 
        AccountingStatement = 7, // Бухгалтерская отчетность за последний отчетный период
        CopyOfPassport = 8, // Копия паспорта лица, входящего в органы управления
        LicenseAgreementBank = 9, // Лицензионный договор / договор оказания услуг
        LicenseAgreementSellerBuyer = 9, // Лицензионный договор / договор оказания услуг / агентский договор по сбору платежей за верификацию 
        //UserRegistrationDocument = 10
    }
}