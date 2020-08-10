using eFMS.API.Catalogue.Service.Models;
using System;

namespace eFMS.API.Catalogue.DL.Models
{
    public class CatContractImportModel : CatContract
    {
        public string CustomerId { get; set; }
        public string PaymentTermTrialDay { get; set; }
        public string CustomerIdError { get; set; }
        public string ContractNoError { get; set; }
        public string AgreementTypeError { get; set; }
        public string SaleServiceError { get; set; }
        public string CompanyError { get; set; }
        public string OfficeError { get; set; }
        public string PaymentMethodError { get; set; }
        public string VasError { get; set; }
        public string SalesmanError { get; set; }
        public string PaymentTermError { get; set; }
        public string CreditLimitedError { get; set; }
        public string CreditLimitRateError { get; set; }
        public string ActiveError { get; set; }
        public string Company { get; set; }
        public string Office { get; set; }
        public DateTime? EffectDate { get; set; }
        public string EffectDateError { get; set; }
        public string ExpiredtDateError { get; set; }
        public string CreditLimitError { get; set; }

        public DateTime? ExpireDate { get; set; }
        public string Salesman { get; set; }
        public string CreditLimited { get; set; }
        public string CreditLimitedRated { get; set; }

        public bool IsValid { get; set; }
        public string Status { get; set; }

    }
}
