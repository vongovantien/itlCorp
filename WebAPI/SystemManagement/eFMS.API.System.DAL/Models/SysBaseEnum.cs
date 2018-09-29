using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysBaseEnum
    {
        public SysBaseEnum()
        {
            SysBaseEnumDetail = new HashSet<SysBaseEnumDetail>();
        }

        public string Key { get; set; }
        public string Api { get; set; }
        public string Description { get; set; }
        public string ColumnsId { get; set; }
        public string ColumnsText { get; set; }
        public string ColumnsDisplay { get; set; }
        public string Query { get; set; }
        public string Server { get; set; }
        public string Version { get; set; }

        public ICollection<SysBaseEnumDetail> SysBaseEnumDetail { get; set; }
    }
}
