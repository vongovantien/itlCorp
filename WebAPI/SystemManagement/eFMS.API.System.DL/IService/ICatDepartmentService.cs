using System;
using System.Collections.Generic;
using System.Text;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.IService
{
    public interface ICatDepartmentService : IRepositoryBase<CatDepartment, CatDepartmentModel>
    {
    }
}
