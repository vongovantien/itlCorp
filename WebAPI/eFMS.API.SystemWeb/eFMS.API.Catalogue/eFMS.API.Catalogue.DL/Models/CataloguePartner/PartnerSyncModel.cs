using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models.CataloguePartner
{
    public class PartnerBankAccountSyncModel
    {
        public string BankCode { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string SwiftCode { get; set; }
        public string Address { get; set; }
    }

    public class PartnerAttachDocSyncModel
    {
        public string AttachDocRowId { get; set; }
        public DateTime? AttachDocDate { get; set; }
        public string AttachDocName { get; set; }
        public string AttachDocPath { get; set; }
    }

    public class PartnerSyncModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string CustomerType { get; set; }
        public string Address { get; set; }
        public string BillingAddress { get; set; }
        public string TaxRegNo { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public string IdCardNo { get; set; }
        public DateTime? IdCardDate { get; set; }
        public string IdCardPlace { get; set; }
        public List<PartnerBankAccountSyncModel> Details { get; set; }
        public List<PartnerAttachDocSyncModel> AtchDocInfo { get; set; }
    }
}
