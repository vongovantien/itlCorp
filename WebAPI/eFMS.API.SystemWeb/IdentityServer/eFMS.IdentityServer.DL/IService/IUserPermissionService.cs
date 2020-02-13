using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.IdentityServer.DL.IService
{
    public interface IUserPermissionService
    {
        List<UserPermissionModel> Get(string userId, Guid officeId);
        Task<List<string>> GetPermission(string userId, Guid officeId);

    }
}
