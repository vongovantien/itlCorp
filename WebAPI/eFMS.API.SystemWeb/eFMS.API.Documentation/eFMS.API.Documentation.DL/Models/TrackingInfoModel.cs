using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class TrackInfoModel
    {
        public DateTime? Plan_Date { get; set; }
        public DateTime? Actual_Date { get; set; }
        public string Event { get; set; }
        public string Station { get; set; }
        public string Flight_Number { get; set; }
        public string Status { get; set; }
        public string Piece { get; set; }
        public string Weight { get; set; }
    }
}
