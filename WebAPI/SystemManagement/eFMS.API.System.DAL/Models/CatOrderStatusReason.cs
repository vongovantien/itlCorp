using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatOrderStatusReason
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public short StatusId { get; set; }
        public string ReasonDescriptionVn { get; set; }
        public string ReasonDescriptionEn { get; set; }
        public string Stage { get; set; }
        public string CustomerStatusVn { get; set; }
        public string CustomerStatusEn { get; set; }

        public SysStatus Status { get; set; }
    }
}
