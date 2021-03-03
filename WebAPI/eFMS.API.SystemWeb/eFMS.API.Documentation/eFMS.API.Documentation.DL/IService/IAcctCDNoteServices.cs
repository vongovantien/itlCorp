using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.Exports;
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
        AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo, List<AcctCdnoteModel> acctCdNoteList = null);
        Crystal Preview(AcctCDNoteDetailsModel model, bool isOrigin);
        AcctCDNoteExportResult GetDataExportOpsCDNote(AcctCDNoteDetailsModel model, Guid officeId);
        bool CheckAllowDelete(Guid cdNoteId);
        Crystal PreviewSIF(AcctCDNoteDetailsModel data, string currency);
        Crystal PreviewAir(AcctCDNoteDetailsModel data, string currency);
        List<CDNoteModel> Paging(CDNoteCriteria criteria, int page, int size, out int rowsCount);
        HandleState RejectCreditNote(RejectCreditNoteModel model);
        AcctCDNoteDetailsModel GetDataPreviewCDNotes(List<AcctCdnoteModel> acctCdNoteList);
        Crystal PreviewOPSCDNoteWithCurrency(PreviewCdNoteCriteria criteria);
    }
}
