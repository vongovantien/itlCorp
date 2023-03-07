using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.DL.Models.CatalogueBank
{
    public class RequestBankModel
    {
        public ACTION Action { get; set; }

        public CatBankModel PartnerBank { get; set; }
    }
    public class BankInForModel
    {
        public string CustomerCode { get; set; }
        public BankDetail Details { get; set; }
        public List<AttachedDocument> AtchDocInfo { get; set; }
    }

    public class BankDetail
    {
        public string BankCode { get; set; }
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string SwiftCode { get; set; }
        public string Address { get; set; }
    }

    public class AttachedDocument
    {
        public string AttachDocRowId { get; set; }
        public DateTime? AttachDocDate { get; set; }
        public string AttachDocName { get; set; }
        public string AttachDocPath { get; set; }
    }
    public enum ACTION
    {
        ADD,
        UPDATE
    }
}
