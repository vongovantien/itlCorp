using eFMS.API.Catalogue.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatPartnerBankModel : CatPartnerBank
    {
        public string BankName { get; set; }
        public string BankCode { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
    }
}
