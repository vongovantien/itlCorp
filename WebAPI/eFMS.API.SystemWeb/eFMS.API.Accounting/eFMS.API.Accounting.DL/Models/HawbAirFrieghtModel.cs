using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class HawbAirFrieghtModel
    {
        public string JobNo { get; set; }
        public string FlightNo { get; set; }
        public DateTime? ShippmentDate { get; set; }
        public string AOL { get; set; }
        public string Mawb { get; set; }
        public string AOD { get; set; }
        public string Service { get; set; }
        public int? Pcs { get; set; }
        public decimal? GW { get; set; }
        public decimal? CW { get; set; }
        public decimal? Rate { get; set; }
        public decimal? AirFreight { get; set; }
        public decimal? FuelSurcharge { get; set; }
        public decimal? WarriskSurcharge { get; set; }
        public decimal? ScreeningFee { get; set; }
        public decimal? AWB { get; set; }
        public decimal? DAN { get; set; }
        public decimal? AMS { get; set; }
        public decimal? OTH { get; set; }
        public decimal? HandlingFee { get; set; }
        public decimal? NetAmount { get; set; }
        public decimal? ExchangeRate { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
