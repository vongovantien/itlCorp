using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace eFMS.API.Documentation.DL.Models
{
    public class WorkOrderRequest: CsWorkOrder
    {
        [Required]
        public List<CsWorkOrderPriceModel> ListPrice { get; set; }
    }

    public class ActiveInactiveRequest
    {
        public Guid Id { get; set; }
        public bool Active { get; set; }
    }
}
