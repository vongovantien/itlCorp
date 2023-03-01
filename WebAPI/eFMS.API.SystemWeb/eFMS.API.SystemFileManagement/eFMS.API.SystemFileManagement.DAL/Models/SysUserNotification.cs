﻿using System;

namespace eFMS.API.SystemFileManagement.Service.Models
{
    public partial class SysUserNotification
    {
        public Guid Id { get; set; }
        public Guid? NotitficationId { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
    }
}
