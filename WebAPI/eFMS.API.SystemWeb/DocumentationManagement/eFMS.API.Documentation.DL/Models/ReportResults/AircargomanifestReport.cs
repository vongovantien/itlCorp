using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.ReportResults
{
    public class AircargomanifestReport
    {
        public string Billype { get; set; }
        public string HWBNO { get; set; }
        public string Pieces { get; set; }
        public decimal GrossWeight { get; set; }
        public string ShipperName { get; set; }
        public string Consignees { get; set; }
        public string Description { get; set; }
        public string FirstDest { get; set; }
        public string SecondDest { get; set; }
        public string ThirdDest { get; set; }
        public string Notify { get; set; }
    }
    public class AircargomanifestParameter
    {
        public string AWB { get; set; }
        public string Marks { get; set; }
        public string Flight { get; set; }
        public string PortLading { get; set; }
        public string PortUnlading { get; set; }
        public string FlightDate { get; set; }
        public string Shipper { get; set; }
        public string Consignee { get; set; }
        public string Contact { get; set; }
    }
}
