using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class CatPotentialModel
    {
        //
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }
        //
        public Guid Id { get; set; }
        public string NameEn { get; set; }
        public string NameLocal { get; set; }
        public string Taxcode { get; set; }
        public string Tel { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public bool? Active { get; set; }
        public decimal? Margin { get; set; }
        public int? Quotation { get; set; }
        public string PotentialType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
    }
}
