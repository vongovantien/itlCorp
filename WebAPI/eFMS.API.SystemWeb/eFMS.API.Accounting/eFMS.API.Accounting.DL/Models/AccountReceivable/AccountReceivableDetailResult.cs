using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountReceivable
{
    public class AccountReceivableDetailResult
    {
        public AccountReceivableResult AccountReceivable { get; set; }
        public List<AccountReceivableGroupOfficeResult> AccountReceivableGrpOffices { get; set; }
    }
}
