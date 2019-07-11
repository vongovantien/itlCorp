using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Operation.Service.ViewModels
{
    public partial class vw_catProvince
    {
        public Guid ID { get; set; }
        public string Code { get; set; }
        public string Name_EN { get; set; }
        public string Name_VN { get; set; }
        public string AreaID { get; set; }
        public Nullable<short> CountryID { get; set; }
        public string CountryNameVN { get; set; }
        public string CountryNameEN { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public Nullable<DateTime> DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public Nullable<DateTime> DatetimeModified { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<DateTime> InactiveOn { get; set; }
    }
}
