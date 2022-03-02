namespace eFMSWindowService
{
    internal class sp_GetOverDuePayment
    {
        public string PartnerName { get; set; }
        public string Office { get; set; }
        public string Service { get; set; }
        public decimal UnpaidAmountVnd { get; set; }
        public decimal UnpaidAmountUsd { get; set; }
    }
}