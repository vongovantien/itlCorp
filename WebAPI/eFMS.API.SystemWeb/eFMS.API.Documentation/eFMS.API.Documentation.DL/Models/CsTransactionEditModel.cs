using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.Service.Models;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsTransactionEditModel: CsTransaction
    {
        public TransactionTypeEnum TransactionTypeEnum { get; set; }
        public List<CsMawbcontainerModel> CsMawbcontainers { get; set; }
        public List<CsDimensionDetailModel> DimensionDetails { get; set; }
    }
}
