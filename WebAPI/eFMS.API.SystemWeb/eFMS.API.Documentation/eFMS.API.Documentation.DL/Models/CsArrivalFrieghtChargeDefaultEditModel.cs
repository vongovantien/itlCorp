using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsArrivalFrieghtChargeDefaultEditModel
    {
        public TransactionTypeEnum Type { get; set; }
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public List<CsArrivalFrieghtChargeDefault> CsArrivalFrieghtChargeDefaults { get; set; }
        public string HblId { get; set; }
    }
}
