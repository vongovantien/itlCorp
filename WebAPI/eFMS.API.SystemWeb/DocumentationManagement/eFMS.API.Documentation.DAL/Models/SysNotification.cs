using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysNotification
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string UrlReference { get; set; }
        public short? ReceiverGroup { get; set; }
        public string ReveiverUser { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserCreated { get; set; }
    }
}
