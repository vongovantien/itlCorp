using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Provider.Infrasture.API.Catalogue
{
    public static class CatCommodityAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/CatCommonity/getAll";
    }
}
