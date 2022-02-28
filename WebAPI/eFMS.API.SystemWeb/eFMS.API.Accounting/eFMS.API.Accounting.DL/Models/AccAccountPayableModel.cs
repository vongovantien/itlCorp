using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAccountPayableModel : AccAccountPayable
    {
        public string RefId { get; set; }
        public string PartnerName { get; set; }
        public string AccountNo { get; set; }
    }
}
