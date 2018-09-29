using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsDocument
    {
        public Guid Id { get; set; }
        public string ReferenceObject { get; set; }
        public string DocType { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public byte[] Icon { get; set; }
        public string FileDescription { get; set; }
        public string FileCheckSum { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public Guid BranchId { get; set; }
    }
}
