using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IAcctCDNoteServices : IRepositoryBase<AcctCdnote, AcctCdnoteModel>
    {
        HandleState AddNewCDNote(AcctCdnoteModel model);
        HandleState UpdateCDNote(AcctCdnoteModel model);
        HandleState DeleteCDNote(Guid idCDNote);
        List<object> GroupCDNoteByPartner(Guid JobId,bool IsHouseBillID);
        AcctSOADetailsModel GetCDNoteDetails(Guid JobId, string CDNoteCode);
        Crystal Preview(AcctSOADetailsModel model);

    }
}
