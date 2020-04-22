using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsShippingInstructionReportModel: CsShippingInstructionModel
    {
        public List<CsTransactionDetailModel> CsTransactionDetails { get; set; }
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
    }
}
