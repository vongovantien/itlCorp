using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysTemplateDetail
    {
        public string Templateid { get; set; }
        public int OrdinalPosition { get; set; }
        public string ColumnName { get; set; }
        public string IsNullable { get; set; }
        public string DataType { get; set; }
        public int? CharacterMaximumLength { get; set; }
        public bool? Invisible { get; set; }
        public string Display { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Lookup { get; set; }
        public bool? Readonly { get; set; }
        public bool? Required { get; set; }
        public int? Stt { get; set; }
        public string Class { get; set; }
        public bool? Hidden { get; set; }
        public bool? Edit { get; set; }

        public SysTemplate Template { get; set; }
    }
}
