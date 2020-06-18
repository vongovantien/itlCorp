using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class SetUnlockRequestApprove
    {
        public Guid Id { get; set; }
        public Guid? UnlockRequestId { get; set; }
        public string Leader { get; set; }
        public string LeaderApr { get; set; }
        public DateTime? LeaderAprDate { get; set; }
        public string Manager { get; set; }
        public string ManagerApr { get; set; }
        public DateTime? ManagerAprDate { get; set; }
        public string Accountant { get; set; }
        public string AccountantApr { get; set; }
        public DateTime? AccountantAprDate { get; set; }
        public string Buhead { get; set; }
        public string BuheadApr { get; set; }
        public DateTime? BuheadAprDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string Comment { get; set; }
        public bool? IsDeny { get; set; }
    }
}
