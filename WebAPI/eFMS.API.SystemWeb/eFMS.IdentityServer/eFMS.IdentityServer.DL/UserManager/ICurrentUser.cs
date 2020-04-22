using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.DL.UserManager
{
    public interface ICurrentUser
    {
        string UserID { get; }
        string UserName { get; }
        Guid CompanyID { get; }
        Guid OfficeID { get; }
        int? DepartmentId { get; }
        short? GroupId { get; }
        List<UserPermissionModel> UserPermissions { get; }
        UserPermissionModel UserMenuPermission { get; set; }
    }
}
