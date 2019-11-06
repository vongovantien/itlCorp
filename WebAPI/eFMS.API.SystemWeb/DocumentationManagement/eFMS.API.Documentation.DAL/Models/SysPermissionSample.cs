using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysPermissionSample
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public short? RoleId { get; set; }
        public string Type { get; set; }
        public bool? Active { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
