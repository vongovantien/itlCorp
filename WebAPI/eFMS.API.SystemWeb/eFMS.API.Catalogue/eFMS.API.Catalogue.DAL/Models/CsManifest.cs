using System;
using System.Collections.Generic;

namespace eFMS.API.Catalogue.Service.Models
{
    public partial class CsManifest
    {
        public Guid JobId { get; set; }
        public string RefNo { get; set; }
        public string Supplier { get; set; }
        public string Attention { get; set; }
        public string MasksOfRegistration { get; set; }
        public string VoyNo { get; set; }
        public Guid? Pol { get; set; }
        public Guid? Pod { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string Consolidator { get; set; }
        public string DeConsolidator { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public string PaymentTerm { get; set; }
        public string ManifestIssuer { get; set; }
        public string UserCreated { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UserModified { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string ManifestShipper { get; set; }
    }
}
