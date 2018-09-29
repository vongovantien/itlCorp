using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsDtbhireTransportRequestApproval
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public string PaymentRefNo { get; set; }
        public string SupplierId { get; set; }
        public string HeadId { get; set; }
        public string HeadStatus { get; set; }
        public DateTime? HeadDate { get; set; }
        public string HeadNotes { get; set; }
        public int? Status { get; set; }
        public Guid? RouteId { get; set; }
        public decimal? BuyingPrice { get; set; }
        public decimal? TotalSurcharge { get; set; }
        public string NotesCs { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
