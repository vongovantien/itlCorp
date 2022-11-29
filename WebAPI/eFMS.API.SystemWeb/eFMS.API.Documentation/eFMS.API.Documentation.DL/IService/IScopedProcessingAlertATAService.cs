using eFMS.API.Documentation.Service.ViewModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IScopedProcessingAlertATDService 
    {
        Task AlertATD(CancellationToken stoppingToken);
        List<vw_GetShipmentAlertATD> GetAlertATDData();
    }
}
