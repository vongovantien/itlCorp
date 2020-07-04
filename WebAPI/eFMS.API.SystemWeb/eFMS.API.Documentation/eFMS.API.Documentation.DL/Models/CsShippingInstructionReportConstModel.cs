using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShippingInstructionReportConstModel : CsShippingInstructionModel
    {
        public List<CsTransactionDetail> CsTransactionDetails { get; set; }
    }
}
