using System;

namespace eFMS.API.Operation.DL.Models.Ecus
{
    public class D_OLA
    {
        public decimal D_OLAID { get; set; }
        public Nullable<decimal> SOTK { get; set; }
        public Nullable<DateTime> NGAY_KB { get; set; }
        public string MA_DV { get; set; }
        public string MA_CANGNN { get; set; }
        public string SO_VAN_DON { get; set; }
        public Nullable<decimal> TRLUONG { get; set; }
        public string MA_DVT { get; set; }
        public Nullable<decimal> THE_TICH { get; set; }
        public Nullable<decimal> LUONG { get; set; }
        public Nullable<int> SO_CONTAINER { get; set; }
        public string PLUONG { get; set; }
        public string _XorN { get; set; }
        public string TEN_DVXK { get; set; }
        public string MA_HIEU_PTVC { get; set; }
    }
}
