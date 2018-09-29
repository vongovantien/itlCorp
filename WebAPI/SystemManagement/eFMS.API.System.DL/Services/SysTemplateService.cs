using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.StoreProcedure;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class SysTemplateService : RepositoryBase<SysTemplate, SysTemplateModel>, ISysTemplateService
    {
        
        ISysTemplateDetailService _sysTemplateDetailService;
        public SysTemplateService(IContextBase<SysTemplate> repository,
            ISysTemplateDetailService sysTemplateDetailService,
            IMapper mapper) : base(repository, mapper)
        {
            _sysTemplateDetailService = sysTemplateDetailService;
        }

        public List<SP_GENNERATE_SYSTEMPLATE_DETAIL> generateTemplateDetail(string id)
        {
            var parameters = new[]{
            new SqlParameter(){ ParameterName="@TEMPLATEID", Value=id },
        };
            return DataContext.DC.ExecuteProcedure<SP_GENNERATE_SYSTEMPLATE_DETAIL>(parameters);
        }
      

        public bool deleteDetail(SysTemplateModel sysTemplateModel)
        {
            var rs=_sysTemplateDetailService.Delete(i => i.Templateid == sysTemplateModel.Id);
            return (_sysTemplateDetailService.Count(i => i.Templateid == sysTemplateModel.Id)>0?false:true);
        }
    }
}
