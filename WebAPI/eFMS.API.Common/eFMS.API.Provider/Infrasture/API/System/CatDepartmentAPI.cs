using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Provider.Infrasture.API.System
{
    public class CatDepartmentAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/CatDepartment";
    }
}
