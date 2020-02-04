using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysUserPermissionGeneral
    {
        public Guid Id { get; set; }
        public string MenuId { get; set; }
        public string UserId { get; set; }
        public Guid? OfficeId { get; set; }
        public bool? Access { get; set; }
        public string Detail { get; set; }
        public string Write { get; set; }
        public string Delete { get; set; }
        public string List { get; set; }
        public bool? Import { get; set; }
        public bool? Export { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
