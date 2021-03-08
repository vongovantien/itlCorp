using System;

namespace eFMS.API.Accounting.DL.ViewModel
{
    public class ClearChargeSoaTable
    {
        public Guid? Id { get; set; }
        public string Soano { get; set; }
        public string PaySoano { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
