using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class SysPartnerApi
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public string ApiKey { get; set; }
        public string Environment { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool? Active { get; set; }
        public Guid? UserId { get; set; }
        public Guid? UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public Guid? UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
