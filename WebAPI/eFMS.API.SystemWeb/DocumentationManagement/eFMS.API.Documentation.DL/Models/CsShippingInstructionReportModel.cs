using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShippingInstructionReportModel: CsShippingInstructionModel
    {
        public List<CsTransactionDetailModel> CsTransactionDetails { get; set; }
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
    }
}
