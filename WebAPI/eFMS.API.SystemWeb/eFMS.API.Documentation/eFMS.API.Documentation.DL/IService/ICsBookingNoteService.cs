using eFMS.API.Common;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsBookingNoteService : IRepositoryBase<CsBookingNote, CsBookingNoteModel>
    {
        List<CsBookingNoteModel> Paging(CsBookingNoteCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CsBookingNoteModel> Query(CsBookingNoteCriteria criteria);
        object AddCsBookingNote(CsBookingNoteEditModel model);
        HandleState UpdateCsBookingNote(CsBookingNoteEditModel model);
        HandleState DeleteCsBookingNote(Guid bookingNoteId);
        CsBookingNoteModel GetDetails(Guid id);



    }
}
