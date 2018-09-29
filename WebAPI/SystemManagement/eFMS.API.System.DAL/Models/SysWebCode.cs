using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysWebCode
    {
        public Guid Id { get; set; }
        public string ReferencedObjectId { get; set; }
        public string ObjectType { get; set; }
        public string ToUser { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserCreated { get; set; }
    }
}
