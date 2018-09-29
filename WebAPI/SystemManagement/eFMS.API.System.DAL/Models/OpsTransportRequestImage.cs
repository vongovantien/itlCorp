using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsTransportRequestImage
    {
        public Guid Id { get; set; }
        public long OrderDetailTrequestId { get; set; }
        public string FileName { get; set; }
        public byte[] Image { get; set; }
        public string FileDescription { get; set; }
        public string FileCheckSum { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
