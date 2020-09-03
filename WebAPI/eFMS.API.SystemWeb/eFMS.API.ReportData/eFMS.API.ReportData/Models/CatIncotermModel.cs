using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatIncotermModel
    {
        //
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        //
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Service { get; set; }
        public string NameEn { get; set; }
        public string NameLocal { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionLocal { get; set; }
        public bool? Active { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserCreated { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
