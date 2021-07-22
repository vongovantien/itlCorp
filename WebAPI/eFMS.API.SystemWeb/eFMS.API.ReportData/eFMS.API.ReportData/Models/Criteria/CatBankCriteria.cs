using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.ReportData.Models
{
    public class CatBankCriteria
    {
        public string All { get; set; }
        public string Id { get; set; }
        public string BankName { get; set; }
        public bool IsDefault { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InActiveOn { get; set; }
    }
}
