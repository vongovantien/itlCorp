using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatServiceTypeMapping
    {
        public int Id { get; set; }
        public short? ServiceTypeId { get; set; }
        public short? ServiceTypeIId { get; set; }
        public string MappingNameVn { get; set; }
        public string MappingNameEn { get; set; }
        public bool? IsCod { get; set; }
    }
}
