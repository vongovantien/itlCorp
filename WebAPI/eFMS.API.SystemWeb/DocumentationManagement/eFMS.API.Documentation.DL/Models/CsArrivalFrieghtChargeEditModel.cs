using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsArrivalFrieghtChargeEditModel
    {
        public Guid HBLID { get; set; }
        public string ArrivalNo { get; set; }
        public DateTime ArrivalFirstNotice { get; set; }
        public DateTime ArrivalSecondNotice { get; set; }
        public string ArrivalHeader { get; set; }
        public string ArrivalFooter { get; set; }
        public List<CsArrivalFrieghtCharge> CsArrivalFrieghtCharges { get; set; }
    }
}
