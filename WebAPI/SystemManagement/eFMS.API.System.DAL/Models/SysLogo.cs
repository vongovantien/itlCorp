using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysLogo
    {
        public Guid WorkPlaceId { get; set; }
        public string ReportName { get; set; }
        public byte[] Logo { get; set; }
        public string Title { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string Type { get; set; }
        public string Notes { get; set; }
    }
}
