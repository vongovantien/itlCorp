using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatTownWard
    {
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public string DistrictNameEN { get; set; }
        public string ProvinceNameEN { get; set; }
        public string CountryNameEN { get; set; }
        public bool? Active { get; set; }
    }
}
