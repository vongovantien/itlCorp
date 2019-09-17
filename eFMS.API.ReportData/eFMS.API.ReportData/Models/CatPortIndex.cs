namespace eFMS.API.ReportData.Models
{
    public class CatPortIndex
    {
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string ModeOfTransport { get; set; }
        public string AreaNameEN { get; set; }
        public short? CountryNameEN { get; set; }
        public bool? Inactive { get; set; }
    }
}
