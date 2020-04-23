using System;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class SysAuthorizationCriteria
    {
        public string All { get; set; }
        public string Service { get; set; }
        public string UserID { get; set; }
        public string AssignTo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? Active { get; set; }
    }
}
