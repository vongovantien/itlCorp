using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class AccAccountingManagementModel: AccAccountingManagement
    {
        public List<ChargeOfAccountingManagementModel> Charges { get; set; }
        public string UserNameCreated { get; set; }
        public string UserNameModified { get; set; }
        public PermissionAllowBase Permission { get; set; }
    }
}
