//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace eFMSWindowService.Models
{
    using System;
    
    public partial class sp_GetShipmentInThreeDayToSendARDept_Result
    {
        public System.Guid ID { get; set; }
        public string JobNo { get; set; }
        public string PartnerName { get; set; }
        public string MAWB { get; set; }
        public string HWBNo { get; set; }
        public Nullable<System.DateTime> ETD { get; set; }
        public Nullable<System.DateTime> ETA { get; set; }
        public string CustomerID { get; set; }
        public string FreightPayment { get; set; }
        public Nullable<System.Guid> OfficeID { get; set; }
        public string OfficeName { get; set; }
    }

    public partial class sp_GetOverDuePayment_Result
    {
        public System.Guid ID { get; set; }
        public string BranchName_EN { get; set; }
        public string PartnerName_EN { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public decimal Debit_Rate { get; set; }
        public decimal NonOverdue { get; set; }
        public decimal Over1To15Day { get; set; }
        public decimal Over16To30Day { get; set; }
        public decimal Over30Day { get; set; }
    }


    public partial class sp_GetOverDuePaymentCredit_Result
    {
        public System.Guid ID { get; set; }
        public string BranchName_EN { get; set; }
        public string PartnerName_EN { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public decimal CreditRate { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal DebitAmount { get; set; }
    }
}
