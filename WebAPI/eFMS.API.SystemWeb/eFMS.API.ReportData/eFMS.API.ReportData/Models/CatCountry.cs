using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public partial class CatCountry
    {
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public bool? Active { get; set; }
    }
}
