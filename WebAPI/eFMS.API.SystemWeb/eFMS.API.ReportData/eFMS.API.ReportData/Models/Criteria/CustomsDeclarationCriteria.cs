using System;

namespace eFMS.API.ReportData.Models
{
    public class CustomsDeclarationCriteria
    {
        public string ClearanceNo { get; set; }
        public DateTime? FromClearanceDate { get; set; }
        public DateTime? ToClearanceDate { get; set; }
        public bool? ImPorted { get; set; }
        public DateTime? FromImportDate { get; set; }
        public DateTime? ToImportDate { get; set; }
        public string CusType { get; set; }
        public string PersonHandle { get; set; }
    }
}
