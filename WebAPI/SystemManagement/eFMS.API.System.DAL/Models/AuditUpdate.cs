using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class AuditUpdate
    {
        public Guid ActionId { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string KeyValue { get; set; }
    }
}
