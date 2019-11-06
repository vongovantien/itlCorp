using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsArrivalFrieghtChargeDefaultEditModel
    {
        public string TransactionType { get; set; }
        public string UserDefault { get; set; }
        public List<CsArrivalFrieghtChargeDefault> CsArrivalFrieghtChargeDefaults { get; set; }
    }
}
