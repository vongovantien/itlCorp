using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceDtbrateCardBookingSchedule
    {
        public int Id { get; set; }
        public Guid RateCardDetailId { get; set; }
        public string EveryDaysInWeek { get; set; }
        public int? EveryDaysInMonth { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
