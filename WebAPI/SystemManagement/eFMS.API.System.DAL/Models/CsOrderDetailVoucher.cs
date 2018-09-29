using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsOrderDetailVoucher
    {
        public int Id { get; set; }
        public Guid OrderDetailId { get; set; }
        public string Code { get; set; }
        public bool? ComeWithCargo { get; set; }
        public bool? Saving { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
