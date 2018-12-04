using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Models
{
    public class CatUnit
    {
        public Guid Id { get; set; }
        public PropertyCommon PropertyCommon { get; set; }
        public CatUnitEntity NewObject { get; set; }
    }
    public class CatUnitEntity
    {
        public short Id { get; set; }
        public string Code { get; set; }
        public string UnitNameVn { get; set; }
        public string UnitNameEn { get; set; }
        public string UnitType { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionVn { get; set; }
    }
}
