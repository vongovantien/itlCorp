using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysPermissionGeneralDetail
    {
        public short Id { get; set; }
        public short? PermissionId { get; set; }
        public bool? Access { get; set; }
        public string Detail { get; set; }
        public string Write { get; set; }
        public string Delete { get; set; }
        public string List { get; set; }
        public bool? Import { get; set; }
        public bool? Export { get; set; }
    }
}
