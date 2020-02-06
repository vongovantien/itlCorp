using eFMS.IdentityServer.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.UserManager
{
    public interface ICurrentUser
    {
        string UserID { get; }
        string EmployeeID { get; }
        string UserName { get; }
        EmployeeModel CurrentEmployee { get; }
        Guid CompanyID { get; }
        Guid OfficeID { get; }
        int DepartmentId { get; }
        short GroupId { get; }
    }
}
