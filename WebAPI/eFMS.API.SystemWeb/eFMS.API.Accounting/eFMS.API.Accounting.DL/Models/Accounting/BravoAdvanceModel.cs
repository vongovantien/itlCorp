using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Accounting
{
    public class BravoAdvanceModel
    {
        public string DataType { get => "ADVANCE"; set => DataType = value; }
        public string TransCode { get; set;}
        public string BranchCode { get; set; }
        public Guid Stt { get; set; }
        public string OfficeCode { get; set; }
        public DateTime? DocDate { get; set; }
        public string ReferenceNo { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string CurrencyCode { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Description0 { get; set; }
        public List<BravoAdvanceRequestModel> Details { get; set; }

    }

    public class BravoAdvanceRequestModel
    {
        public string ChargeType { get => "CREDIT"; set => ChargeType = value; }
        public string NganhCode { get => "FWD"; set => NganhCode = value; }
        public Guid RowId { get; set; }
        public string Ma_SpHt { get; set; }  // JOB No
        public string BillEntryNo { get; set; } // HBL
        public string MasterBillNo { get; set; } // MBL
        public decimal? OriginalAmount { get; set; }
        public string Description { get; set; }
        public string DeptCode { get; set; }  // - SEA:ITLCS| AIR:ITLAIR | Custom Logistic: ITLOPS


    }
}
