using System;

namespace eFMS.API.Operation.DL.Models.Criteria
{
    public class CustomsDeclarationCriteria
    {
        public string ClearanceNo { get; set; }
        public DateTime? FromClearanceDate { get; set; }
        public DateTime? ToClearanceDate { get; set; }
        public bool? ImPorted { get; set; }
        public DateTime? FromImportDate { get; set; }
        public DateTime? ToImportDate { get; set; }
        public string Type { get; set; }
        public string PersonHandle { get; set; }
        public string CustomerNo { get; set; }
    }
}
