using eFMS.API.Accounting.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAccountingManagementResult : AccAccountingManagement
    {
        public string PartnerName { get; set; }
        public string CreatorName { get; set; }
    }
}
