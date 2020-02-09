using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.IService
{
    public interface IUserPermissionService
    {
        List<UserPermissionModel> Get(string userId, Guid officeId);
    }
}
