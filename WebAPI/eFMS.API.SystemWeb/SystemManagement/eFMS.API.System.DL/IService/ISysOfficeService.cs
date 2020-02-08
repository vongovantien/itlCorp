using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysOfficeService : IRepositoryBase<SysOffice, SysOfficeModel>
    {
        IQueryable<SysOffice> GetOffices();

        List<SysOfficeViewModel> Query(SysOfficeCriteria employee);
        IQueryable<SysOfficeViewModel> Paging(SysOfficeCriteria criteria, int pageNumber, int pageSize, out int rowsCount);

        HandleState AddOffice(SysOfficeModel sysOffice);
        HandleState UpdateOffice(SysOfficeModel sysOffice);
        HandleState DeleteOffice(Guid id);
        IQueryable<SysOfficeViewModel> GetOfficeByCompany(Guid id);

        List<SysOffice> GetOfficePermission(string username, Guid companyId);



    }
}
