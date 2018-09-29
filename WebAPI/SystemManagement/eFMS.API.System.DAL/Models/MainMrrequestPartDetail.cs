using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainMrrequestPartDetail
    {
        public Guid Id { get; set; }
        public Guid MrrequestDetaiId { get; set; }
        public string OldSerial { get; set; }
        public string NewSerial { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
