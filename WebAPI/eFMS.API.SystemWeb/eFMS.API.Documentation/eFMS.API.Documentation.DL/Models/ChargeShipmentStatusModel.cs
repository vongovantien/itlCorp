using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ChargeShipmentStatusModel
    {
        public Guid JobId { get; set; }
        public string Status { get; set; }
        public string TransitionType { get; set; }
    }
}
