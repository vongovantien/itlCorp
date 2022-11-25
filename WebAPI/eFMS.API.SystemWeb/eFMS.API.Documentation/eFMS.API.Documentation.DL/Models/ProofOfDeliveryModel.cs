using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class ProofOfDeliveryModel
    {
        public List<Guid> HouseBills { get; set; }
        public string ReferenceNo { get; set; }
        public string DeliveryPerson { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Note { get; set; }
    }
}
