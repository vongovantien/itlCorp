using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Provider.Infrasture.API.Catalogue
{
    public class CatPartnerAPI
    {
        public static string GetAll(string baseUri) => $"{baseUri}/CatPartner";
    }
}
