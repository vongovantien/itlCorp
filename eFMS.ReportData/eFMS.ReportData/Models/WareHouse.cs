using System;

namespace eFMS.ReportData.Models
{
    public partial class WareHouse
    {
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Address { get; set; }
        public Guid? DistrictName { get; set; }
        public Guid? ProvinceName { get; set; }
        public short? CountryName { get; set; }
        public bool? Inactive { get; set; }
    }
}
