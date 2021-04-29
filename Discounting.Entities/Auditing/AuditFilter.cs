using System;

namespace Discounting.Entities.Auditing
{
    public class AuditFilter
    {
        public int? Id { get; set; }
        public IncidentType? Incident { get; set; }
        public Guid? UserId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? SourceId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? Until { get; set; }
        public IncidentResult IncidentResult { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 100;
    }
}