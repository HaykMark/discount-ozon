using System;
using Discounting.API.Common.ViewModels.Common;
using Discounting.Entities.Auditing;

namespace Discounting.API.Common.ViewModels
{
    public class AuditDTO : DTO<int>
    {
        public IncidentType Incident { get; set; }
        public Guid? SourceId { get; set; }
        public string UserName { get; set; }
        public string CompanyShortName { get; set; }
        public Guid? UserId { get; set; }
        public DateTime IncidentDate { get; set; }
        public string Tin { get; set; }
        public Guid? CompanyId { get; set; }
        public string IpAddress { get; set; }
        public string Message { get; set; }
        public IncidentResult IncidentResult  { get; set; }
    }
}