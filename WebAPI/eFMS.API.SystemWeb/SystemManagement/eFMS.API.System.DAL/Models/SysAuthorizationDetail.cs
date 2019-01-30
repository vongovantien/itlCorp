using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysAuthorizationDetail
    {
        public int Id { get; set; }
        public int AuthorizationId { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string MenuId { get; set; }
        public short PermissionId { get; set; }
        public short? OtherIntructionId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual SysAuthorization Authorization { get; set; }
    }
}
