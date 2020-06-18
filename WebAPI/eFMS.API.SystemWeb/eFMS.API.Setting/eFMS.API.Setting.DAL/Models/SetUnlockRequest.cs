using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SetUnlockRequest
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Requester { get; set; }
        public string UnlockType { get; set; }
        public DateTime? NewServiceDate { get; set; }
        public string GeneralReason { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestUser { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
