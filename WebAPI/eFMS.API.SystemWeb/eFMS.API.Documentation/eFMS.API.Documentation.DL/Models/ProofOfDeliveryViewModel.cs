using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ProofOfDeliveryViewModel
    {
        public Guid HblId { get; set; }
        public string ReferenceNo { get; set; }
        public string DeliveryPerson { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Note { get; set; }
    }
}
