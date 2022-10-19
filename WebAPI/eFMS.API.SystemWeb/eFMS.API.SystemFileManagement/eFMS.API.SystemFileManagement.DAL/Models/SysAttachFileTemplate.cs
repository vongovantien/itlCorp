using System;
using System.Collections.Generic;

namespace eFMS.API.SystemFileManagement.Service.Models
{
    public partial class SysAttachFileTemplate
    {
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
        public bool? Active { get; set; }
        public bool? Required { get; set; }
        public string TransactionType { get; set; }
        public string Service { get; set; }
        public string ServiceType { get; set; }
        public string PreFix { get; set; }
        public string SubFix { get; set; }
        public string Type { get; set; }
        public string StorageRule { get; set; }
        public int? StorageTime { get; set; }
        public string StorageType { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid? UserCreated { get; set; }
        public Guid? UserModified { get; set; }
        public string StorageFollowing { get; set; }
        public string Tag { get; set; }
        public int Id { get; set; }
    }
}
