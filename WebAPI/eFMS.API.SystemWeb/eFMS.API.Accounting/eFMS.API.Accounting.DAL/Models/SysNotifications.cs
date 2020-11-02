using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class SysNotifications
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string ActionLink { get; set; }
        public bool? IsClosed { get; set; }
        public bool? IsRead { get; set; }
        public string Title { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
