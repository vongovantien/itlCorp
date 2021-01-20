using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class GeneralReportCriteria : ICloneable
    {
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        public DateTime? ServiceDateFrom { get; set; }
        public DateTime? ServiceDateTo { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public string CustomerId { get; set; }
        public string Service { get; set; }
        public string Currency { get; set; }
        public string JobId { get; set; }
        public string Mawb { get; set; }
        public string Hawb { get; set; }
        public string OfficeId { get; set; }
        public string DepartmentId { get; set; }
        public string GroupId { get; set; }
        public string PersonInCharge { get; set; }
        public string SalesMan { get; set; }
        public string Creator { get; set; }
        public string CarrierId { get; set; }
        public string AgentId { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
    }
}
