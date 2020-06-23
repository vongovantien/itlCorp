using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SetLockingDateShipment
    {
        public int Id { get; set; }
        public Guid? OfficeId { get; set; }
        public string ServiceType { get; set; }
        public byte? LockDate { get; set; }
        public byte? LockAfterUnlocking { get; set; }
        public bool? IsApply { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
