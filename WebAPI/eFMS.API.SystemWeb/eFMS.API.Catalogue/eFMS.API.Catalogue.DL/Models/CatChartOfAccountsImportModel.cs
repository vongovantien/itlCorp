using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatChartOfAccountsImportModel : CatChartOfAccounts
    {
        public string Status { get; set; }
        public string AccountCodeError { get; set; }
        public string AccountNameEnError { get; set; }
        public string StatusError { get; set; }

        public string AccountNameLocalError { get; set; }
        public bool IsValid { get; set; }

    }
}
