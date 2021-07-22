using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.Service.Models
{
    public partial class AcctReceiptSync
    {
        public Guid Id { get; set; }
        public Guid? ReceiptId { get; set; }
        public string ReceiptSyncNo { get; set; }
        public string Type { get; set; }
        public DateTime? LastSyncDate { get; set; }
        public string ReasonReject { get; set; }
        public string SyncStatus { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
