using System;

namespace eFMS.API.Documentation.Service.ViewModels
{
    public class vw_GetShipmentAlertATA
    {
        public string JobNo { get; set; }
        public string Service { get; set; }
        public string PIC { get; set; }
        public DateTime ETA { get; set; }
        public string Email { get; set; }
        public string EmailCC { get; set; }
    }
}
