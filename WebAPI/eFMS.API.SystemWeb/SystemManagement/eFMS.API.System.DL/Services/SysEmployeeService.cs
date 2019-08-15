using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.System.DL.Models.Criteria;
using ITL.NetCore.Common;
using eFMS.API.System.Service.Contexts;

namespace eFMS.API.System.DL.Services
{
    public class SysEmployeeService : RepositoryBase<SysEmployee, SysEmployeeModel>, ISysEmployeeService
    {
        public SysEmployeeService(IContextBase<SysEmployee> repository, IMapper mapper) : base(repository, mapper)
        {
            SetChildren<SysUser>("Id", "EmployeeId");
        }

        public HandleState AddEmployee(SysEmployeeModel sysEmployee)
        {
            return DataContext.Add(sysEmployee);
        }

        public HandleState DeleteEmployee(string id)
        {
            return DataContext.Delete(x => x.Id == id);
        }
        public HandleState UpdateEmployee(SysEmployeeModel sysEmployee)
        {
            try
            {
                var emp = ((eFMSDataContext)DataContext.DC).SysEmployee.FirstOrDefault(x => x.Id == sysEmployee.Id);
                if (emp == null)
                {
                    throw new SystemException("Employee not found, maybe wrong ID");
                }
                return DataContext.Update(sysEmployee, x => x.Id == sysEmployee.Id);
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
         
        }

        public List<EmployeeViewModel> Query(EmployeeCriteria employee)
        {
            var employees = DataContext.Get(x => x.Id == employee.Id);
            var workplaces = ((eFMSDataContext)DataContext.DC).CatPlace;
            var results = employees.Join(workplaces, em => em.WorkPlaceId, wp => wp.Id, (em, wp) => new { em, wp })
                .Select(x => new EmployeeViewModel {
                    Id = x.em.Id,
                    WorkPlaceId = x.em.WorkPlaceId,
                    DepartmentId = x.em.DepartmentId,
                    EmployeeNameVn = x.em.EmployeeNameVn,
                    EmployeeNameEn = x.em.EmployeeNameEn,
                    Position = x.em.Position,
                    Birthday = x.em.Birthday,
                    ExtNo = x.em.ExtNo,
                    Tel = x.em.Tel,
                    HomePhone = x.em.HomePhone,
                    HomeAddress = x.em.HomeAddress,
                    Email = x.em.Email,
                    AccessDescription = x.em.AccessDescription,
                    Photo = x.em.Photo,
                    EmpPhotoSize = x.em.EmpPhotoSize,
                    SaleTarget = x.em.SaleTarget,
                    Bonus = x.em.Bonus,
                    SaleResource = x.em.SaleResource,
                    LdapObjectGuid = x.em.LdapObjectGuid,
                    UserCreated = x.em.UserCreated,
                    DatetimeCreated = x.em.DatetimeCreated,
                    UserModified = x.em.UserModified,
                    DatetimeModified = x.em.DatetimeModified,
                    Inactive = x.em.Inactive,
                    InactiveOn = x.em.InactiveOn,
                    Signature = x.em.Signature,
                    DepartmentName = string.Empty,
                    WorkplaceAddress = x.wp.Address
                }).ToList();
            return results;
        }

       
    }
}
