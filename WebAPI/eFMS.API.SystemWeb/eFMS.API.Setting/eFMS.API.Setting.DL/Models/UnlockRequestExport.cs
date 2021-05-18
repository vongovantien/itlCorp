using System;

namespace eFMS.API.Setting.DL.Models
{
    public class UnlockRequestExport
    {
        public string SubjectUnlock { get; set; }
        public string DescriptionUnlock { get; set; }
        public string ReferenceNo { get; set; }
        public string UnlockType { get; set; }
        public DateTime? ChangeServiceDate { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? UnlockDate { get; set; }
        public string Requester { get; set; }
        public string ReasonDetail { get; set; }
        public string GeneralReason { get; set; }
    }
}
