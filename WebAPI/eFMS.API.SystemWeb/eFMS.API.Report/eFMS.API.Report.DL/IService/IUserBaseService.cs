using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Report.DL.IService
{
    public interface IUserBaseService: IRepositoryBase<SysUser, SysUserModel>
    {
        List<string> GetDeptManager(Guid? companyId, Guid? officeId, int? departmentId);
        List<string> GetAccoutantManager(Guid? companyId, Guid? officeId);
        List<string> GetOfficeManager(Guid? companyId, Guid? officeId);
        List<string> GetCompanyManager(Guid? companyId);
    }
}
