using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class CatAgreementModel
    {
        public Guid ID { get; set; }
        public string ContractNo { get; set; }
        public string ContractType { get; set; }
        public string SaleManName { get; set; }
        public decimal CusAdvanceAmount { get; set; }
        public DateTime? ExpiredDate { get; set; }

    }
}
