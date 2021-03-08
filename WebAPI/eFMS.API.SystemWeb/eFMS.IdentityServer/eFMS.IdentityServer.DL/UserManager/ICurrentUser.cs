using eFMS.API.Common.Models;
using System;
using System.Collections.Generic;

namespace eFMS.IdentityServer.DL.UserManager
{
    public interface ICurrentUser
    {
        string UserID { get; set; }
        string UserName { get; set; }
        Guid CompanyID { get; set; }
        Guid OfficeID { get; set; }
        int? DepartmentId { get; set; }
        short? GroupId { get; set; }
        decimal? KbExchangeRate { get; set; }
        List<UserPermissionModel> UserPermissions { get; }
        UserPermissionModel UserMenuPermission { get; set; }
    }
}
