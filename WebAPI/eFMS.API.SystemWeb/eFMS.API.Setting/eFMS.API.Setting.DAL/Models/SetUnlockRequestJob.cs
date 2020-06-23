using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SetUnlockRequestJob
    {
        public Guid Id { get; set; }
        public Guid? UnlockRequestId { get; set; }
        public string UnlockName { get; set; }
        public string Reason { get; set; }
        public string Job { get; set; }
        public string UnlockType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
