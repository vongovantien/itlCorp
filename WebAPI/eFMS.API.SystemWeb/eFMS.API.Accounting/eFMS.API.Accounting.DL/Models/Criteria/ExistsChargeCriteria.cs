using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class ExistsChargeCriteria
    {
        public string partnerId { get; set; }
        public List<string> jobIds { get; set; }
        public List<string> hbls { get; set; }
        public List<string> mbls { get; set; }
        public List<string> customNos { get; set; }
        public List<string> soaNo { get; set; }
        public List<string> creditNo { get; set; }
        public string servicesType { get; set; }
        public DateTime? serviceDateFrom { get; set; }
        public DateTime? serviceDateTo { get; set; }
        public string personInCharge { get; set; }
        public string requester { get; set; }
    }
}
