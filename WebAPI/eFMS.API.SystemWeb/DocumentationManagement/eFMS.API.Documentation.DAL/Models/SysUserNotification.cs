using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysUserNotification
    {
        public Guid Id { get; set; }
        public Guid? NotitficationId { get; set; }
        public bool? IsRead { get; set; }
        public string UserId { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
