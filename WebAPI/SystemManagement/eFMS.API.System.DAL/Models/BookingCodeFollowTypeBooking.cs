using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class BookingCodeFollowTypeBooking
    {
        public string TypeBooking { get; set; }
        public string DateCode { get; set; }
        public string BrandCode { get; set; }
        public int BookingCodeNumber { get; set; }
    }
}
