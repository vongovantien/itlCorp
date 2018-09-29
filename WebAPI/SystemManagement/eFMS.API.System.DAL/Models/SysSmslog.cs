using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysSmslog
    {
        public Guid? WorkPlaceId { get; set; }
        public Guid Id { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string Imei { get; set; }
        public string Infomation { get; set; }
        public string Description { get; set; }
        public string Provider { get; set; }
        public string Status { get; set; }
        public string Tel { get; set; }
        public string LogType { get; set; }
        public string ActionDescription { get; set; }
    }
}
