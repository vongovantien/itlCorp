using System;

namespace eFMS.API.Documentation.DL.Models
{
    public class ProofOfDeliveryModel
    {
        public Guid hblId { get; set; }
        public string ReferenceNo { get; set; }
        public string DeliveryPerson { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Note { get; set; }
    }
}
