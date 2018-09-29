using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysOneTmsbuildVersion
    {
        public short Id { get; set; }
        public string DatabaseVersion { get; set; }
        public DateTime VersionDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
