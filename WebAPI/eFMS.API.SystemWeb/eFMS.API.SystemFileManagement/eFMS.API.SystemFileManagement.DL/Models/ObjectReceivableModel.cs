using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class ObjectReceivableModel
    {
        public Guid? SurchargeId { get; set; }
        public string PartnerId { get; set; }
        public Guid? Office { get; set; }
        public string Service { get; set; }
    }
}
