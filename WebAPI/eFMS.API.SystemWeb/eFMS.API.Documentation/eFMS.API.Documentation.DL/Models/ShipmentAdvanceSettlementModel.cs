using eFMS.API.Documentation.Service.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class ShipmentAdvanceSettlementModel: sp_GetAdvanceSettleOpsTransaction
    {
        public decimal Balance { get; set; }
    }
}
