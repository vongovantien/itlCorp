﻿using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SysImage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Thumb { get; set; }
        public string Url { get; set; }
        public string Folder { get; set; }
        public string ObjectId { get; set; }
        public string ChildId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DateTimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? IsTemp { get; set; }
        public string KeyS3 { get; set; }
        public string SyncStatus { get; set; }
    }
}
