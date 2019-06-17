using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Provider.Infrasture
{
    public class Settings
    {
        public class APIUrls
        {
            public string SystemUrl { get; set; }
            public string CatelogueUrl { get; set; }
            public string DocumentationUrl { get; set; }
            public string SettingUrl { get; set; }
        }
    }
}