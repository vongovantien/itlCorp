using System;

namespace eFMS.API.ForPartner.DL.ViewModel
{
    public class RejectChargeTable
    {
        public Guid? Id { get; set; }
        public string SyncedFrom { get; set; }
        public string PaySyncedFrom { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
