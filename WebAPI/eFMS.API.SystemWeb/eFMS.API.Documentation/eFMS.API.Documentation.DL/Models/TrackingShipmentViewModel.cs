using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class TrackingShipmentViewModel
    {
        public string ColoaderName { get; set; }
        public string FlightNo { get; set; }
        public DateTime? FlightDate { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        public List<SysTrackInfoModel> TrackInfos { get; set; }

    }
}
