using System;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class FormPLsheetReport
    {
        public string COSTING { get; set; }
        public string TransID { get; set; }	
        public DateTime TransDate { get; set; }
        public string HWBNO { get; set; }
        public string MAWB { get; set; }	
        public string PartnerName { get; set; }
        public string ContactName { get; set; }	
        public string ShipmentType { get; set; }
        public string NominationParty { get; set; }	
        public bool Nominated { get; set; }
        public string POL { get; set; }
        public string POD { get; set; }	
        public string Commodity { get; set; }
        public string Volumne { get; set; }
        public string Carrier { get; set; }
        public string Agent { get; set; }
        public string ATTN { get; set; }	
        public string Consignee { get; set; }
        public string ContainerNo { get; set; }
        public string OceanVessel { get; set; }
        public string LocalVessel { get; set; }
        public string FlightNo { get; set; }
        public string SeaImpVoy { get; set; }	
        public string LoadingDate { get; set; }
        public string ArrivalDate { get; set; }
        public string FreightCustomer { get; set; }
        public decimal FreightColoader { get;set; }
        public string PayableAccount { get; set; }
        public string Description { get; set; }
        public string Curr { get; set; }
        public decimal VAT { get; set; }
        public decimal VATAmount { get; set; }
        public decimal Cost { get; set; }
        public decimal Revenue { get; set; }
        public decimal Exchange { get; set; }
        public decimal VNDExchange { get; set; }
        public bool Paid { get; set; }
        public DateTime DatePaid { get; set; }
        public string Docs { get; set; }	
        public string Notes { get; set; }
        public string InputData { get; set; }	
        public decimal SalesProfit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; }	
        public string LastRevised { get; set; }
        public bool OBH { get; set; }
        public decimal ExtRateVND { get; set; }
        public bool KBck { get; set; }
        public bool NoInv { get; set; }
        public string Approvedby { get; set; }	
        public DateTime ApproveDate { get; set; }
        public string SalesCurr { get; set; }
        public decimal? GW { get; set; }
        public decimal MCW { get; set; }
        public decimal? HCW { get; set; }
        public string PaymentTerm { get; set; }
        public string DetailNotes { get; set; }
        public string ExpressNotes { get; set; }
        public string InvoiceNo { get; set; }	
        public string CodeVender { get; set; }	
        public string CodeCus { get; set; }	
        public bool Freight { get; set; }
        public bool Collect { get; set; }
        public string FreightPayableAt { get; set; }	
        public decimal PaymentTime { get; set; }
        public decimal PaymentTimeCus { get; set; }
        public decimal Noofpieces { get; set; }
        public string UnitPieaces { get; set; }
        public string TpyeofService { get; set; }	
        public string ShipmentSource { get; set; }
        public bool RealCost { get; set; }
        public string UnitPriceStr { get; set; }
    }

    public class FormPLsheetReportParameter
    {
        public string Contact { get; set; }
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyAddress1 { get; set; }
        public string CompanyAddress2 { get; set; }
        public string Website { get; set; }
        public int CurrDecimalNo { get; set; }
        public int DecimalNo { get; set; }
        public string HBLList { get; set; }
    }
}
