using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysUserLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Ip { get; set; }
        public string ComputerName { get; set; }
        public DateTime LoggedInOn { get; set; }
        public DateTime? LoggedOffOn { get; set; }
        public Guid? WorkPlaceId { get; set; }
    }
}
