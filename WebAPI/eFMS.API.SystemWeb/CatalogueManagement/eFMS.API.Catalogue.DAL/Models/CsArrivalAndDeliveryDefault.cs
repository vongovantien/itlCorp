using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
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
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
