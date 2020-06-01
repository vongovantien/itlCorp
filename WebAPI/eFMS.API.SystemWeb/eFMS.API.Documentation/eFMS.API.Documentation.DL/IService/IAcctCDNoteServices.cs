using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IAcctCDNoteServices : IRepositoryBase<AcctCdnote, AcctCdnoteModel>
    {
        HandleState AddNewCDNote(AcctCdnoteModel model);
        HandleState UpdateCDNote(AcctCdnoteModel model);
        HandleState DeleteCDNote(Guid idCDNote);
        List<object> GroupCDNoteByPartner(Guid Id, bool IsShipmentOperation);
        AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo);
        Crystal Preview(AcctCDNoteDetailsModel model);
        bool CheckAllowDelete(Guid cdNoteId);
        Crystal PreviewSIF(PreviewCdNoteCriteria criteria);
        Crystal PreviewAir(PreviewCdNoteCriteria criteria);
        List<CDNoteModel> Paging(CDNoteCriteria criteria, int page, int size, out int rowsCount);
    }
}
