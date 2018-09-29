using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;

using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ISysBaseEnumService: IRepositoryBase<SysBaseEnum, SysBaseEnumModel>
    {
        Dictionary<int, string> Authorization();
        Task<object> databaseInfo();
        List<sp_getBaseEnum> getBaseEnum(string id);
        List<SysBaseEnumModel> GetSysBaseEnums();
        Dictionary<string, string> None();
        HandleState PostSysBaseEnums(SysBaseEnumModel sysBaseEnumModel);
        HandleState PutSysBaseEnums(SysBaseEnumModel sysBaseEnumModel, Expression<Func<SysBaseEnumModel, bool>> predicate);
        string ToString();
        List<vw_catDistrict> vw_catDistrict();
        List<vw_catHub> vw_catHub();
        List<vw_datatype> vw_Datatypes();
        Dictionary<string, string> YesNo();

        SysBaseEnumModel getFirst(string id);
    }
}