using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class vw_GetShipmentAlertATD
    {
        public string JobNo { get; set; }
        public string Service { get; set; }
        public string PIC { get; set; }
        public DateTime ETD { get; set; }
        public string Email { get; set; }
        public string EmailCC { get; set; }
    }
}
