namespace eFMS.API.Setting.DL.Models
{
    public class CustomClearanceImportModel : CustomsDeclarationModel
    {
        public bool IsValid { get; set; }
        public string ClearanceDateStr { get; set; }
        public bool ClearanceDateValid { get; set; }
        public string GrossWeightStr { get; set; }
        public bool GrossWeightValid { get; set; }
        public string NetWeightStr { get; set; }
        public bool NetWeightValid { get; set; }
        public string CbmStr { get; set; }
        public bool CbmValid { get; set; }
        public string PcsStr { get; set; }
        public bool PcsValid { get; set; }
        public string QtyContStr { get; set; }
        public bool QtyContValid { get; set; }

        public bool ClearanceNoValid { get; set; }
        public bool TypeValid { get; set; }
        public bool PartnerTaxCodeValid { get; set; }
        public bool MblidValid { get; set; }
        public bool HblidValid { get; set; }
        public bool GatewayValid { get; set; }

        public bool CargoTypeValid { get; set; }
        public bool ServiceTypeValid { get; set; }
        public bool RouteValid { get; set; }
        public bool CommodityValid { get; set; }
        public string CommodityName { get; set; }

        public bool ImportCountryCodeValid { get; set; }
        public bool ExportCountryCodeValid { get; set; }
    }
}
