﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsArrivalAndDeliveryDefault
    {
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public string ArrivalHeader { get; set; }
        public string ArrivalFooter { get; set; }
        public string Doheader1 { get; set; }
        public string Doheader2 { get; set; }
        public string Dofooter { get; set; }
    }
}
