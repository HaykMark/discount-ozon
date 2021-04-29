using System;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;
using Discounting.Extensions;

namespace Discounting.Logics.Excel
{
    public class ExcelUserProfileRegulationReport : IExcelReport
    {
        public ExcelUserProfileRegulationReport(UserRegulation userRegulation)
        {
            var user = userRegulation.User;
            var company = userRegulation.User.Company;

            //User fields
            SignPermission = user.CanSign ? "Б" : "А";
            UserPosition = user.Position;
            UserPhone = user.Phone;
            UserEmail = user.Email;
            UserLastName = user.Surname;
            UserFirstName = user.FirstName;
            UserSecondName = user.SecondName;

            //Company fields
            CurrentDate = DateTime.Now.ToRussianDateFormat();
            CompanyShortName = company.ShortName;
            CompanyFullName = company.FullName;
            IncorporationForm = company.IncorporationForm;
            PSRN = company.PSRN;
            TIN = company.TIN;
            KPP = company.KPP;
            
            if(!user.CanSign)
                return;

            //Regulation fields
            AuthorizedUserDate = userRegulation.UserProfileRegulationInfo.Date.ToRussianDateFormat();
            AuthorizedUserNumber = userRegulation.UserProfileRegulationInfo.Number;
            AuthorizedUserCitizenship = userRegulation.UserProfileRegulationInfo.Citizenship;
            AuthorizedUserPlaceOfBirth = userRegulation.UserProfileRegulationInfo.PlaceOfBirth;
            AuthorizedUserDateOfBirth = userRegulation.UserProfileRegulationInfo.DateOfBirth.ToRussianDateFormat();
            AuthorizedUserAuthorityValidityDate =
                userRegulation.UserProfileRegulationInfo.AuthorityValidityDate?.ToRussianDateFormat();
            AuthorizedUserIdentityDocument = userRegulation.UserProfileRegulationInfo.IdentityDocument;

            ContactAddress = company.CompanyContactInfo.Address;
            ContactOrganizationAddress = company.CompanyContactInfo.OrganizationAddress;
            if (userRegulation.UserProfileRegulationInfo.IsResident)
            {
                AuthorizedUserPassportSeries = userRegulation.UserProfileRegulationInfo.PassportSeries;
                AuthorizedUserPassportDate =
                    userRegulation.UserProfileRegulationInfo.PassportDate?.ToRussianDateFormat();
                AuthorizedUserPassportNumber = userRegulation.UserProfileRegulationInfo.PassportNumber;
                AuthorizedUserPassportUnitCode = userRegulation.UserProfileRegulationInfo.PassportUnitCode;
                AuthorizedUserPassportIssuingAuthorityPSRN =
                    userRegulation.UserProfileRegulationInfo.PassportIssuingAuthorityPSRN;
                AuthorizedUserPassportSNILS = userRegulation.UserProfileRegulationInfo.PassportSNILS;
            }
            else
            {
                AuthorizedUserMigrationCardRightToResideDocument =
                    userRegulation.UserProfileRegulationInfo.MigrationCardRightToResideDocument;
                AuthorizedUserMigrationCardAddress = userRegulation.UserProfileRegulationInfo.MigrationCardAddress;
                AuthorizedUserMigrationCardRegistrationAddress = userRegulation.UserProfileRegulationInfo.MigrationCardRegistrationAddress;
                AuthorizedUserMigrationCardPhone = userRegulation.UserProfileRegulationInfo.MigrationCardPhone;
            }
        }

        public string CurrentDate { get; }
        public string CompanyShortName { get; }
        public string CompanyFullName { get; }
        public string IncorporationForm { get; }
        public string PSRN { get; }
        public string TIN { get; }
        public string KPP { get; }
        public string ContactAddress { get; }
        public string ContactOrganizationAddress { get; }

        public string SignPermission { get; }
        public string UserPosition { get; }
        public string UserPhone { get; }
        public string UserEmail { get; }
        public string UserLastName { get; }
        public string UserFirstName { get; }
        public string UserSecondName { get; }

        public string AuthorizedUserDate { get; }
        public string AuthorizedUserNumber { get; }
        public string AuthorizedUserCitizenship { get; }
        public string AuthorizedUserPlaceOfBirth { get; }
        public string AuthorizedUserDateOfBirth { get; }
        public string AuthorizedUserAuthorityValidityDate { get; }
        public string AuthorizedUserIdentityDocument { get; }

        public string AuthorizedUserPassportSeries { get; }
        public string AuthorizedUserPassportDate { get; }
        public string AuthorizedUserPassportNumber { get; }
        public string AuthorizedUserPassportUnitCode { get; }
        public string AuthorizedUserPassportIssuingAuthorityPSRN { get; }
        public string AuthorizedUserPassportSNILS { get; }

        public string AuthorizedUserMigrationCardRightToResideDocument { get; }
        public string AuthorizedUserMigrationCardAddress { get; }
        public string AuthorizedUserMigrationCardRegistrationAddress { get; }
        public string AuthorizedUserMigrationCardPhone { get; }
    }
}