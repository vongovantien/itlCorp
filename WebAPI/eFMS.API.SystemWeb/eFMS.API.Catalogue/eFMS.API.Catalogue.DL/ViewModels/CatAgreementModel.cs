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
        public string SaleManId { get; set; }
        public string SaleManName { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string CreditCurrency { get; set; }
        public string CurrencyId { get; set; }
        public decimal? CustomerAdvanceAmountVnd { get; set; }
        public decimal? CustomerAdvanceAmountUsd { get; set; }
        public string SaleService { get; set; }
    }
}
