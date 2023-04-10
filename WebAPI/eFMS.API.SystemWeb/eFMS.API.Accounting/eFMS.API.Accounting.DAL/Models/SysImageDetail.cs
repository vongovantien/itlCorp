using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class SysImageDetail
    {
        public Guid Id { get; set; }
        public string SystemFileName { get; set; }
        public string UserFileName { get; set; }
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserCreated { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public decimal? GroupId { get; set; }
        public Guid? OfficeId { get; set; }
        public int? DepartmentId { get; set; }
        public string UserModified { get; set; }
        public Guid? Hblid { get; set; }
        public Guid? JobId { get; set; }
        public int? DocumentTypeId { get; set; }
        public string Source { get; set; }
        public Guid? SysImageId { get; set; }
        public string Note { get; set; }
        public Guid? GenEdocId { get; set; }
    }
}
