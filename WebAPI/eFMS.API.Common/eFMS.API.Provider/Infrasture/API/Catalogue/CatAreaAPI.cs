using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Infrasture.API.Catalogue
{
    public static class CatAreaAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/CatArea";
    }
}
