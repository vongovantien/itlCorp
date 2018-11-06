using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class UserGroupCriteria
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Decription { get; set; }
        public string UserCreated { get; set; }
        public bool? Inactive { get; set; }
    }
}
