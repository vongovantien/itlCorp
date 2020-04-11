using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Provider.Infrasture.API.System
{
    public class SysUserAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/SysUser";
    }
}
