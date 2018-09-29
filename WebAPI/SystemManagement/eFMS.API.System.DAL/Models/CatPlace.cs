using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatPlace
    {
        public CatPlace()
        {
            CsReceptacleChecking = new HashSet<CsReceptacleChecking>();
            SysUser = new HashSet<SysUser>();
            SysUserOtherWorkPlace = new HashSet<SysUserOtherWorkPlace>();
            SysUserRole = new HashSet<SysUserRole>();
        }

        public Guid Id { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string DisplayName { get; set; }
        public string GeoCode { get; set; }
        public string PlaceTypeId { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CsReceptacleChecking> CsReceptacleChecking { get; set; }
        public ICollection<SysUser> SysUser { get; set; }
        public ICollection<SysUserOtherWorkPlace> SysUserOtherWorkPlace { get; set; }
        public ICollection<SysUserRole> SysUserRole { get; set; }
    }
}
