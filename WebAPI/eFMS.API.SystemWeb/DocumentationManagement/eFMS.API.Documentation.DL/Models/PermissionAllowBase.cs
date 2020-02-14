using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class PermissionAllowBase
    {
        public bool AllowUpdate { get; set; } = false;
        public bool AllowDelete { get; set; } = false;
        public bool AllowAddCharge { get; set; } = false;
        public bool AllowUpdateCharge { get; set; } = false;
        public bool AllowLock { get; set; } = false;
    }
}
