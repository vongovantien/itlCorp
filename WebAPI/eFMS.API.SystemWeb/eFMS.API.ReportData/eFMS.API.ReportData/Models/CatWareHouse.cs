using System;


namespace eFMS.API.ReportData.Models
{
    public partial class CatWareHouse
    {
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Address { get; set; }
        public string DistrictName { get; set; }
        public string ProvinceName { get; set; }
        public short? CountryName { get; set; }
        public bool? Active { get; set; }
    }
}
