using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysUserPermission
    {
        public Guid Id { get; set; }
        public Guid PermissionSampleId { get; set; }
        public string UserId { get; set; }
        public Guid OfficeId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
