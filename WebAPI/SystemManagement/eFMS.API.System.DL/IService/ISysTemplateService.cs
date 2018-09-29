using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.StoreProcedure;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ISysTemplateService: IRepositoryBase<SysTemplate, SysTemplateModel>
    {
        List<SP_GENNERATE_SYSTEMPLATE_DETAIL> generateTemplateDetail(string id);
        bool deleteDetail(SysTemplateModel sysTemplateModel);
    }
}