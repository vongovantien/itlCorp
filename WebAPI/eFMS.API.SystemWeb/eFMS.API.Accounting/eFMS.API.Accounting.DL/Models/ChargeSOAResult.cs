using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models
{
    public class ChargeSOAResult
    {
        public Guid ID { get; set; }
        public Guid HBLID { get; set; }
        public Guid ChargeID { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string Type { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public string SOANo { get; set; }
        public bool IsOBH { get; set; } 
        public string Currency { get; set; }
        public string InvoiceNo { get; set; }
        public string Note { get; set; }
        public string CustomerID { get; set; }
        public DateTime? ServiceDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? InvoiceIssuedDate { get; set; }
        public string TransactionType { get; set; }
        public string UserCreated { get; set; }
        public decimal Quantity { get; set; }
        public short UnitId { get; set; }
        public string Unit { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? VATRate { get; set; }
        public string CreditDebitNo { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? CommodityGroupID { get; set; }
        public string Commodity { get; set; }

        public string Service { get; set; }
        public string CDNote { get; set; }
        public string TaxCodeOBH { get; set; }

        public decimal? GrossWeight { get; set; }
        public decimal? ChargeWeight { get; set; }

        public string FlightNo { get; set; }
        public DateTime? ShippmentDate { get; set; }

        public Guid? AOL { get; set; }
        public Guid? AOD { get; set; }
        public int? PackageQty { get; set; }

        public decimal? FinalExchangeRate { get; set; }

        public DateTime? ExchangeDate { get; set; }

        public decimal? CBM { get; set; }

        public string PackageContainer { get; set; }

        public string TypeCharge { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? NetAmount { get; set; }

        public string PIC { get; set; }






    }
}
