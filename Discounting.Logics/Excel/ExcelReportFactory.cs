using System;
using Discounting.Common.Types;
using Discounting.Entities;
using Discounting.Entities.Account;
using Discounting.Entities.CompanyAggregates;
using Discounting.Entities.Regulations;

namespace Discounting.Logics.Excel
{
    public static class ExcelFactory
    {
        public static IExcelReport CreateExcelReport(IEntity<Guid> entity, ExcelReportType type)
        {
            return type switch
            {
                ExcelReportType.Registry
                when entity is Registry registry => new ExcelRegistryReport(registry),
                ExcelReportType.ProfileRegulation
                when entity is Company company => new ExcelAdminProfileRegulationReport(company),
                ExcelReportType.UserProfileRegulation 
                when entity is UserRegulation userRegulation  => new ExcelUserProfileRegulationReport(userRegulation),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}