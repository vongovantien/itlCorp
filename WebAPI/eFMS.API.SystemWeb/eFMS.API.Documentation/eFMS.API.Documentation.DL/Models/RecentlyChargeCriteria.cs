using eFMS.API.Documentation.DL.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class RecentlyChargeCriteria
    {
        public Guid CurrentJobId { get; set; }
        public string PersonInCharge { get; set; }
        public TransactionTypeEnum TransactionType { get; set; }
        public string ShippingLine { get; set; }
        public string CustomerId { get; set; }
        public string ConsigneeId { get; set; }
        public Guid? POL { get; set; }
        public Guid? POD { get; set; }
        public string ChargeType { get; set; }
    }
}
