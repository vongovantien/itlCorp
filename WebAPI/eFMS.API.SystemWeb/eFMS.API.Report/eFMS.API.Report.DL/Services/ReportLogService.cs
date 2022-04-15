using AutoMapper;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Report.DL.Services
{
    public class ReportLogService : RepositoryBase<SysReportLog, SysReportLogModel>, IReportLogService
    {
        public ReportLogService(IContextBase<SysReportLog> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState WriteLogReport(SysReportLog model)
        {
            return DataContext.Add(model);
        }
    }
}
