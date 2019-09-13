using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctCDNoteServices : IRepositoryBase<AcctCdnote, AcctCdnoteModel>
    {
        HandleState AddNewCDNote(AcctCdnoteModel model);
        HandleState UpdateCDNote(AcctCdnoteModel model);
        HandleState DeleteCDNote(Guid idCDNote);
        List<object> GroupCDNoteByPartner(Guid JobId,bool IsHouseBillID);
        AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo);
        Crystal Preview(AcctCDNoteDetailsModel model);

    }
}
