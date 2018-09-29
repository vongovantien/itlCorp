using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainVehicleMaintenance
    {
        public Guid Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public Guid MrrequestId { get; set; }
        public string RequestedVehicleType { get; set; }
        public int? ContermetNumber { get; set; }
        public string BillNo { get; set; }
        public string Remark { get; set; }
        public int? RepairLevelId { get; set; }
        public decimal? Amount { get; set; }
        public string Type { get; set; }
        public string ApprovedManager { get; set; }
        public DateTime? ApprovedDatetime { get; set; }
        public int? ApprovedStatus { get; set; }
        public int? MaintenanceTypeId { get; set; }
        public DateTime? FinishedDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
