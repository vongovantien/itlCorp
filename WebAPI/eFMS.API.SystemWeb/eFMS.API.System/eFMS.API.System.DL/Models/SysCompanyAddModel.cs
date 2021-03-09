using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace eFMS.API.System.DL.Models
{
    public class SysCompanyAddModel
    {
        public Guid Id { get; set; }
        public string CompanyCode { get; set; }
        [Required]
        public string CompanyNameEn { get; set; }
        [Required]
        public string CompanyNameVn { get; set; }
        [Required]
        public string CompanyNameAbbr { get; set; }
        public string Website { get; set; }
        public string PhotoName { get; set; }
        public string PhotoUrl { get; set; }
        public bool Status { get; set; }
        public decimal? KbExchangeRate { get; set; }

    }
}
