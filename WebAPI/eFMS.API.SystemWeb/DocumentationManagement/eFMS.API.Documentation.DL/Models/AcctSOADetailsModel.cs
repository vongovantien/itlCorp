using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class AcctSOADetailsModel
    {
        public string PartnerNameEn { get; set; }
        public string PartnerShippingAddress { get; set; }
        public string PartnerTel { get; set; }
        public string PartnerTaxcode { get; set; }
        public string PartnerId { get; set; }
        public string HbLadingNo { get; set; }
        public string MbLadingNo { get; set; }
        public Guid JobId { get; set; }
        public string Pol { get; set; }
        public string PolName { get; set; }
        public string PolCountry { get; set; }
        public string Pod { get; set; }
        public string PodName { get; set; }
        public string PodCountry { get; set; }
        public string Vessel { get; set; }
        public string HbConstainers { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public bool IsLocked { get; set; }
        public decimal? Volum { get; set; }
        public List<CsShipmentSurchargeDetailsModel> ListSurcharges { get; set; }
        public AcctSoa Soa { get; set; }
        public decimal? TotalCredit { get; set; }
        public decimal? TotalDebit { get; set; }
    }
}
