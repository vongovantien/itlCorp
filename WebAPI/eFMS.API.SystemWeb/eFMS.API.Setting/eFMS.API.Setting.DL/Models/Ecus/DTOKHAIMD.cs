using System;

namespace eFMS.API.Setting.DL.Models.Ecus
{
    public class DTOKHAIMD
    {
        public decimal DToKhaiMDID { get; set; }
        public string XorN { get; set; }
        public Nullable<decimal> SOTK { get; set; }
        public string SOTK_DAU_TIEN { get; set; }
        public Nullable<DateTime> NGAY_DK { get; set; }
        public string MA_DV { get; set; }
        public string VAN_DON { get; set; }
        public string MA_CK { get; set; }
        public string MA_CANGNN { get; set; }
        public string NUOC_XK { get; set; }
        public string NUOC_NK { get; set; }
        public Nullable<decimal> TR_LUONG { get; set; }
        public Nullable<decimal> SO_KIEN { get; set; }
        public string DVT_KIEN { get; set; }
        public Nullable<decimal> SO_CONTAINER { get; set; }
        public string PLUONG { get; set; }
        public string MA_HIEU_PTVC { get; set; }
    }
}
