using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysTemplate
    {
        public SysTemplate()
        {
            SysTemplateDetail = new HashSet<SysTemplateDetail>();
        }

        public string Id { get; set; }
        public string TableName { get; set; }
        public string TableCatalog { get; set; }
        public string TableSchema { get; set; }
        public bool? Inactive { get; set; }
        public string Userid { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string TableType { get; set; }
        public string Api { get; set; }
        public string AddApi { get; set; }
        public string EditApi { get; set; }
        public string DeleteApi { get; set; }
        public string Server { get; set; }
        public string Version { get; set; }

        public ICollection<SysTemplateDetail> SysTemplateDetail { get; set; }
    }
}
