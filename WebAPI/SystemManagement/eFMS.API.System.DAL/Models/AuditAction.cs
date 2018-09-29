using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AuditAction
    {
        public Guid Id { get; set; }
        public string TableName { get; set; }
        public string UserId { get; set; }
        public DateTime? Datetime { get; set; }
        public string Type { get; set; }
        public string WorkPlace { get; set; }
    }
}
