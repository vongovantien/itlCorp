using System;

namespace SystemManagement.DL.Models.Views
{
    public class vw_catDistrict
    {
        public Guid DistrictID { get; set; }

        public string Code { get; set; }

        public string Name_VN { get; set; }

        public string Name_EN { get; set; }

        public Guid? ProvinceID { get; set; }

        public string ProvinceName { get; set; }

        public short? CountryID { get; set; }

        public string Note { get; set; }

        public string UserCreated { get; set; }

        public DateTime? DatetimeCreated { get; set; }

        public string UserModified { get; set; }

        public DateTime? DatetimeModified { get; set; }

        public bool? Inactive { get; set; }

        public DateTime? InactiveOn { get; set; }

    }

}
