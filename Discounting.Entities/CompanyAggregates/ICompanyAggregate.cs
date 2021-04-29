using System;
using Discounting.Common.Types;

namespace Discounting.Entities.CompanyAggregates
{
    public interface ICompanyAggregate : IEntity<Guid>
    {
        public Guid CompanyId { get; set; }
    }
}