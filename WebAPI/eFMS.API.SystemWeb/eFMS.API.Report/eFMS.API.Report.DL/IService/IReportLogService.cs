using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using ITL.NetCore.Connection.BL;


namespace eFMS.API.Report.DL.IService
{
    public interface IReportLogService: IRepositoryBase<SysReportLogModel, SysReportLog>
    {
    }
}
