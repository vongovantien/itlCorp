using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysBuAddModel
    {
        public int Id { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyNameEn { get; set; }
        public string CompanyNameVn { get; set; }
        public string CompanyNameAbbr { get; set; }
        public string Website { get; set; }
        public string PhotoName { get; set; }
        public string PhotoUrl { get; set; }
        public bool Status { get; set; }

    }
}
