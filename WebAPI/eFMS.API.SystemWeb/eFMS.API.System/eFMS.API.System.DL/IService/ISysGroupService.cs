using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysGroupService : IRepositoryBase<SysGroup, SysGroupModel>
    {
        IQueryable<SysGroupModel> Paging(SysGroupCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<SysGroupModel> Query(SysGroupCriteria criteria);
        SysGroupModel GetById(short id);
        IQueryable<SysGroupModel> GetGroupByDepartment(int id);
        List<CatDepartmentGroupCriteria> GetGroupDepartmentPermission(string username , Guid officeId);
    }
}
