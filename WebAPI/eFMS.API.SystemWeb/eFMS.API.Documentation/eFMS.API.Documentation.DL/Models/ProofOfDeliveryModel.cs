using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class ProofOfDeliveryModel
    {
        public Guid HblId { get; set; }
        public string DeliveryPerson { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}
