using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsMawbcontainerModel: CsMawbcontainer
    {
        public string ContainerTypeName { get; set; }
        public string UnitOfMeasureName { get; set; }
        public string CommodityName { get; set; }
        public string PackageTypeName { get; set; }
    }
}
