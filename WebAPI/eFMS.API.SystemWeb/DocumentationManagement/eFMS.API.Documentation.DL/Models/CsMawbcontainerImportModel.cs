using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsMawbcontainerImportModel: CsMawbcontainer
    {
        public string ContainerNoError { get; set; }
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
        public string GwError { get; set; }
        public string NwError { get; set; }
        public string CbmError { get; set; }
        public string PackageQuantityError { get; set; }
        public string DuplicateError { get; set; }
        public string ExistedError { get; set; }
    }
}
