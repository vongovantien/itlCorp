using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsWorkOrder
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Guid? PartnerId { get; set; }
        public Guid? SalesmanId { get; set; }
        public Guid? AgentId { get; set; }
        public Guid? ShipperId { get; set; }
        public Guid? ConsigneeId { get; set; }
        public string AgentDescription { get; set; }
        public string ShipperDescription { get; set; }
        public string ConsigneeDescription { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public string PolDescription { get; set; }
        public string PodDescription { get; set; }
        public Guid? IncotermId { get; set; }
        public string ShipmentType { get; set; }
        public string PaymentMethod { get; set; }
        public string PickupPlace { get; set; }
        public Guid? UserCreated { get; set; }
        public Guid? UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
