using eFMS.API.Documentation.Service.ViewModels;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IScopedProcessingAlertService 
    {
        void AlertATD();
        void AlertATA();
        List<vw_GetShipmentAlertATD> GetAlertATDData();
        List<vw_GetShipmentAlertATA> GetAlertATAData();
    }
}
