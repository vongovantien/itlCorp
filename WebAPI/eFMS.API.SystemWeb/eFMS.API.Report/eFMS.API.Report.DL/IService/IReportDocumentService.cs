using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Report.DL.IService
{
    public interface IReportDocumentService
    {
        IQueryable<AcctCdnote> Query();
    }
}
