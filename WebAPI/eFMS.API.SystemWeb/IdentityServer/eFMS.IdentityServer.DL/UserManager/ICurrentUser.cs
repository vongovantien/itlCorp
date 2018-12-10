using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.DL.UserManager
{
    public interface ICurrentUser
    {
        String UserID { get; }
        //String EmployeeID { get; }
    }
}
