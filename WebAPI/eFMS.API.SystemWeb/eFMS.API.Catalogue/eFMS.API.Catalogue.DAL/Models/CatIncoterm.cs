using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CatIncoterm
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Service { get; set; }
        public string NameEn { get; set; }
        public string NameVn { get; set; }
        public bool? Active { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public Guid? UserCreated { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
