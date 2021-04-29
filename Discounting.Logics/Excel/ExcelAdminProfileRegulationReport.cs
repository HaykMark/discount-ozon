using System;
using System.Linq;
using Discounting.Entities.CompanyAggregates;
using Discounting.Extensions;

namespace Discounting.Logics.Excel
{
    public class ExcelAdminProfileRegulationReport : IExcelReport
    {
        public ExcelAdminProfileRegulationReport(Company company)
        {

            OwnerName = company.OwnerFullName;
            OwnerPosition = company.OwnerPosition;
            CompanyShortName = company.ShortName;
            CompanyFullName = company.FullName;
            OwnerDocument = company.OwnerDocument;
            PaidUpAuthorizedCapitalInformation = company.PaidUpAuthorizedCapitalInformation;
            IncorporationForm = company.IncorporationForm;
            PSRN = company.PSRN;
            TIN = company.TIN;
            KPP = company.KPP;
            RegisteringAuthorityName = company.RegisteringAuthorityName;
            RegistrationStatePlace = company.RegistrationStatePlace;
            StateStatisticsCode = company.StateStatisticsCode;
            if (company.StateRegistrationDate != null)
                StateRegistrationDate = company.StateRegistrationDate.Value.ToRussianDateFormat();

            if (company.HasPowerOfAttorney)
            {
                AuthorizedUserDate = company.CompanyAuthorizedUserInfo.Date.ToRussianDateFormat();
                AuthorizedUserNumber = company.CompanyAuthorizedUserInfo.Number;
                AuthorizedUserFirstName = company.CompanyAuthorizedUserInfo.FirstName;
                AuthorizedUserSecondName = company.CompanyAuthorizedUserInfo.SecondName;
                AuthorizedUserLastName = company.CompanyAuthorizedUserInfo.LastName;
                AuthorizedUserCitizenship = company.CompanyAuthorizedUserInfo.Citizenship;
                AuthorizedUserPlaceOfBirth = company.CompanyAuthorizedUserInfo.PlaceOfBirth;
                AuthorizedUserDateOfBirth = company.CompanyAuthorizedUserInfo.DateOfBirth.ToRussianDateFormat();
                AuthorizedUserAuthorityValidityDate =
                    company.CompanyAuthorizedUserInfo.AuthorityValidityDate.ToRussianDateFormat();
                AuthorizedUserIdentityDocument = company.CompanyAuthorizedUserInfo.IdentityDocument;
            }

            ContactAddress = company.CompanyContactInfo.Address;
            ContactOrganizationAddress = company.CompanyContactInfo.OrganizationAddress;
            ContactPhone = company.CompanyContactInfo.Phone;
            ContactEmail = company.CompanyContactInfo.Email;
            ContactMailingAddress = company.CompanyContactInfo.MailingAddress;
            ContactNameOfGoverningBodies = company.CompanyContactInfo.NameOfGoverningBodies;

            OwnerPositionName = company.CompanyOwnerPositionInfo.Name;
            OwnerPositionDate = company.CompanyOwnerPositionInfo.Date.ToRussianDateFormat();
            OwnerPositionNumber = company.CompanyOwnerPositionInfo.Number;
            OwnerPositionFirstName = company.CompanyOwnerPositionInfo.FirstName;
            OwnerPositionSecondName = company.CompanyOwnerPositionInfo.SecondName;
            OwnerPositionLastName = company.CompanyOwnerPositionInfo.LastName;
            OwnerPositionCitizenship = company.CompanyOwnerPositionInfo.Citizenship;
            OwnerPositionPlaceOfBirth = company.CompanyOwnerPositionInfo.PlaceOfBirth;
            OwnerPositionDateOfBirth = company.CompanyOwnerPositionInfo.DateOfBirth.ToRussianDateFormat();
            OwnerPositionAuthorityValidityDate =
                company.CompanyOwnerPositionInfo.AuthorityValidityDate.ToRussianDateFormat();
            OwnerPositionIdentityDocument = company.CompanyOwnerPositionInfo.IdentityDocument;

            if (company.CompanyOwnerPositionInfo.IsResident)
            {
                var ownerPassportInfo = company.ResidentPassportInfos
                    .FirstOrDefault(p => p.PositionType == CompanyPositionType.Owner);
                OwnerPassportSeries = ownerPassportInfo?.Series;
                OwnerPassportDate = ownerPassportInfo?.Date.ToRussianDateFormat();
                OwnerPassportNumber = ownerPassportInfo?.Number;
                OwnerPassportUnitCode = ownerPassportInfo?.UnitCode;
                OwnerPassportIssuingAuthorityPSRN = ownerPassportInfo?.IssuingAuthorityPSRN;
                OwnerPassportSNILS = ownerPassportInfo?.SNILS;
            }
            else
            {
                var ownerMigrationCard = company.MigrationCardInfos
                    .FirstOrDefault(p => p.PositionType == CompanyPositionType.Owner);
                OwnerPositionMigrationCardRightToResideDocument = ownerMigrationCard?.RightToResideDocument;
                OwnerPositionMigrationCardAddress = ownerMigrationCard?.Address;
                OwnerPositionMigrationCardRegistrationAddress = ownerMigrationCard?.RegistrationAddress;
                OwnerPositionMigrationCardPhone = ownerMigrationCard?.Phone;
            }

            if (company.HasPowerOfAttorney)
            {
                if (company.CompanyAuthorizedUserInfo.IsResident)
                {
                    var authorizedUserPassport = company.ResidentPassportInfos
                        .FirstOrDefault(p => p.PositionType == CompanyPositionType.AuthorizedUser);

                    AuthorizedUserPassportSeries = authorizedUserPassport?.Series;
                    AuthorizedUserPassportDate = authorizedUserPassport?.Date.ToRussianDateFormat();
                    AuthorizedUserPassportNumber = authorizedUserPassport?.Number;
                    AuthorizedUserPassportUnitCode = authorizedUserPassport?.UnitCode;
                    AuthorizedUserPassportIssuingAuthorityPSRN = authorizedUserPassport?.IssuingAuthorityPSRN;
                    AuthorizedUserPassportSNILS = authorizedUserPassport?.SNILS;
                }
                else
                {
                    var authorizedUserMigrationCard = company.MigrationCardInfos
                        .FirstOrDefault(p => p.PositionType == CompanyPositionType.AuthorizedUser);
                    AuthorizedUserMigrationCardRightToResideDocument =
                        authorizedUserMigrationCard?.RightToResideDocument;
                    AuthorizedUserMigrationCardAddress = authorizedUserMigrationCard?.Address;
                    AuthorizedUserMigrationCardRegistrationAddress = authorizedUserMigrationCard?.RegistrationAddress;
                    AuthorizedUserMigrationCardPhone = authorizedUserMigrationCard?.Phone;
                }
            }
        }

        public string OwnerName { get; }
        public string OwnerPosition { get; }
        public string OwnerDocument { get; }
        public string PaidUpAuthorizedCapitalInformation { get; }
        public string CompanyShortName { get; }
        public string CompanyFullName { get; }
        public string IncorporationForm { get; }
        public string PSRN { get; }
        public string TIN { get; }
        public string KPP { get; }
        public string RegisteringAuthorityName { get; set; }
        public string RegistrationStatePlace { get; set; }
        public string StateStatisticsCode { get; set; }
        public string StateRegistrationDate { get; set; }

        public string AuthorizedUserDate { get; }
        public string AuthorizedUserNumber { get; }
        public string AuthorizedUserFirstName { get; }
        public string AuthorizedUserSecondName { get; }
        public string AuthorizedUserLastName { get; }
        public string AuthorizedUserCitizenship { get; }
        public string AuthorizedUserPlaceOfBirth { get; }
        public string AuthorizedUserDateOfBirth { get; }
        public string AuthorizedUserAuthorityValidityDate { get; }
        public string AuthorizedUserIdentityDocument { get; }

        public string ContactAddress { get; }
        public string ContactOrganizationAddress { get; }
        public string ContactPhone { get; }
        public string ContactEmail { get; }
        public string ContactMailingAddress { get; }
        public string ContactNameOfGoverningBodies { get; }

        public string OwnerPositionName { get; }
        public string OwnerPositionDate { get; }
        public string OwnerPositionNumber { get; }
        public string OwnerPositionFirstName { get; }
        public string OwnerPositionSecondName { get; }
        public string OwnerPositionLastName { get; }
        public string OwnerPositionCitizenship { get; }
        public string OwnerPositionPlaceOfBirth { get; }
        public string OwnerPositionDateOfBirth { get; }
        public string OwnerPositionAuthorityValidityDate { get; }
        public string OwnerPositionIdentityDocument { get; }

        public string AuthorizedUserMigrationCardRightToResideDocument { get; }
        public string AuthorizedUserMigrationCardAddress { get; }
        public string AuthorizedUserMigrationCardRegistrationAddress { get; }
        public string AuthorizedUserMigrationCardPhone { get; }
        public string OwnerPositionMigrationCardRightToResideDocument { get; }
        public string OwnerPositionMigrationCardAddress { get; }
        public string OwnerPositionMigrationCardRegistrationAddress { get; }
        public string OwnerPositionMigrationCardPhone { get; }

        public string AuthorizedUserPassportSeries { get; }
        public string AuthorizedUserPassportDate { get; }
        public string AuthorizedUserPassportNumber { get; }
        public string AuthorizedUserPassportUnitCode { get; }
        public string AuthorizedUserPassportIssuingAuthorityPSRN { get; }
        public string AuthorizedUserPassportSNILS { get; }

        public string OwnerPassportSeries { get; }
        public string OwnerPassportDate { get; }
        public string OwnerPassportNumber { get; }
        public string OwnerPassportUnitCode { get; }
        public string OwnerPassportIssuingAuthorityPSRN { get; }
        public string OwnerPassportSNILS { get; }
    }
}