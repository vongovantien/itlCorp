using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models
{
    public partial class CatBank
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string BankNameVn { get; set; }
        public string BankNameEn { get; set; }
        public bool? Active { get; set; }
    }
}
