using System;

namespace eFMS.API.Documentation.DL.Models.Criteria
{
    public class BookingNoteCriteria
    {
        public string ReportType { get; set; }
        public Guid HblId { get; set; }
        public string FlexId {get;set;}
        public string FlightNo2 { get; set; }
        public string ContactPerson { get; set; }
        public string ClosingTime { get; set; }
    }
}
