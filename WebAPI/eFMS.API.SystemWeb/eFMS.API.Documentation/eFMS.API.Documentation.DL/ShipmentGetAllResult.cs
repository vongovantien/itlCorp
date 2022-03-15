using eFMS.API.Documentation.Service.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL
{
    public class ShipmentGetAllResult:sp_GetAllShipment
    {
        public bool Access { get; set; }
    }
}
