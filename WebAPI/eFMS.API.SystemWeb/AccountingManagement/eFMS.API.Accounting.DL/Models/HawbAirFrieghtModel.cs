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
        public double? GW { get; set; }
        public double? CW { get; set; }
        public double? Rate { get; set; }
        public double? AirFreight { get; set; }
        public double? FuelSurcharge { get; set; }
        public double? WarriskSurcharge { get; set; }
        public double? ScreeningFee { get; set; }
        public double? AWB { get; set; }
        public double? HandlingFee { get; set; }
        public double? NetAmount { get; set; }
        public double? ExchangeRate { get; set; }
        public double? TotalAmount { get; set; }
    }
}
