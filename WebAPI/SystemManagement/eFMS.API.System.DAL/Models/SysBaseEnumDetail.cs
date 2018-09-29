using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysBaseEnumDetail
    {
        public string BaseEnumKey { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int? Stt { get; set; }
        public bool? Invisible { get; set; }

        public SysBaseEnum BaseEnumKeyNavigation { get; set; }
    }
}
