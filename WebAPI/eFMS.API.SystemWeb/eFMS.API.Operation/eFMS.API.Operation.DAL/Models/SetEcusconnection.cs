using System;
using System.Collections.Generic;

namespace eFMS.API.Operation.Service.Models
{
    public partial class SetEcusconnection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        public string ServerName { get; set; }
        public string Dbusername { get; set; }
        public string Dbpassword { get; set; }
        public string Dbname { get; set; }
        public string Note { get; set; }
        public bool? Active { get; set; }
        public string InactiveOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public Guid? OfficeId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
