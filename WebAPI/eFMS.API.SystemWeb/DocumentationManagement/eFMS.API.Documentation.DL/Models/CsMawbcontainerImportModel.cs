using eFMS.API.Documentation.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsMawbcontainerImportModel: CsMawbcontainer
    {
        public string ContainerTypeName { get; set; }
        public string UnitOfMeasureName { get; set; }
        public string CommodityName { get; set; }
        public string PackageTypeName { get; set; }
        public bool IsValid { get; set; }
        public string ContainerTypeNameError { get; set; }
        public string QuantityError { get; set; }
        public string PackageTypeNameError { get; set; }
        public string CommodityNameError { get; set; }
        public string UnitOfMeasureNameError { get; set; }
    }
}
