using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsArrivalViewModel
    {
        public Guid HBLID { get; set; }
        public string ArrivalNo { get; set; }
        public DateTime? ArrivalFirstNotice { get; set; }
        public DateTime? ArrivalSecondNotice { get; set; }
        public string ArrivalHeader { get; set; }
        public string ArrivalFooter { get; set; }
        public List<CsArrivalFrieghtChargeModel> CsArrivalFrieghtCharges { get; set; }
    }
}
