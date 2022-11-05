using eFMS.API.SystemFileManagement.Service.Models;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class SysImageDetailModel : SysImageDetail
    {
        public string ImageUrl { get; set; }
        public string HBLNo { get; set; }
        public string JobNo { get; set; }
        public string DocumentTypeName { get; set; }
        public string TransactionType { get; set; }
    }
}
