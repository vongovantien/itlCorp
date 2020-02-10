using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.IService
{
    public interface IAuthenUserService : IRepositoryBase<SysUser, UserModel>
    {
        int Login(string username, string password, Guid companyId, out LoginReturnModel modelReturn);

        UserViewModel GetUserById(string id);
    }
}
