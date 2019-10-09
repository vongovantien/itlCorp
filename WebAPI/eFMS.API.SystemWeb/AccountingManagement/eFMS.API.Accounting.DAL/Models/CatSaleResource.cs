using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class CatSaleResource
    {
        public CatSaleResource()
        {
            SysEmployee = new HashSet<SysEmployee>();
        }

        public string Id { get; set; }
        public string ResourceName { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }

        public virtual ICollection<SysEmployee> SysEmployee { get; set; }
    }
}
