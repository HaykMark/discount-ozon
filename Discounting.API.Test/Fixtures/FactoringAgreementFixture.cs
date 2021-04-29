using System;
using System.Collections.Generic;
using Discounting.API.Common.ViewModels.Company;
using Discounting.Seeding.Constants;
using Discounting.Tests.Fixtures.Common;
using Discounting.Tests.Infrastructure;

namespace Discounting.Tests.Fixtures
{
    public class FactoringAgreementFixture : BaseFixture
    {
        public FactoringAgreementFixture(AppState appState) : base(appState)
        {
        }

        public FactoringAgreementDTO GetPayload(Guid contractorId)
        {
            return new FactoringAgreementDTO
            {
                CompanyId = contractorId,
                BankId = GuidValues.CompanyGuids.BankUserOne,
                FactoringContractDate = DateTime.UtcNow.AddDays(1),
                FactoringContractNumber = "testF",
                BankName = "bank",
                BankCity = "test",
                BankBic = "123456789",
                BankOGRN = "1234567890123",
                BankCheckingAccount = "12345678901234567890",
                BankCorrespondentAccount = "09876543211234567890",
                SupplyFactoringAgreementDtos = new List<SupplyFactoringAgreementDTO>
                {
                    new SupplyFactoringAgreementDTO
                    {
                        Number = "test",
                        Date = DateTime.UtcNow
                    },
                    new SupplyFactoringAgreementDTO
                    {
                        Number = "test2",
                        Date = DateTime.UtcNow
                    }
                }
            };
        }
    }
}