using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysParameter
    {
        public SysParameter()
        {
            SysParameterDetail = new HashSet<SysParameterDetail>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string ParaGroup { get; set; }
        public string ParameterNameVn { get; set; }
        public string ParameterNameEn { get; set; }
        public bool? NoLimitEffect { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<SysParameterDetail> SysParameterDetail { get; set; }
    }
}
