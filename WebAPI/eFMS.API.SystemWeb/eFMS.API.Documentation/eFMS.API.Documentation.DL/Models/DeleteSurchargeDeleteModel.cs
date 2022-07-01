using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class DeleteSurchargeDeleteModel
    {
        public Guid Id { get; set; }
        public Guid Office { get; set; }
        public string PartnerId { get; set; }
        public string Service { get; set; }
    }
}
