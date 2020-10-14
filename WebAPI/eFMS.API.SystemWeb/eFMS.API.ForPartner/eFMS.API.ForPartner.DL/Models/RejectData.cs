namespace eFMS.API.ForPartner.DL.Models
{
    public class RejectData
    {
        public string ReferenceID { get; set; } //ID CDNote, ID Soa, ID Voucher, ID Advance, ID Settlement
        public string Reason { get; set; } //Lý do Reject
        public string Type { get; set; } // CDNOTE, SOA, VOUCHER, ADVANCE, SETTLEMENT
    }
}
