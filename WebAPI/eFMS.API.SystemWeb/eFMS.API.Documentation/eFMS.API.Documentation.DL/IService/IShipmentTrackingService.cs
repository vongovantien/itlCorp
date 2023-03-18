using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IShipmentTrackingService : IRepositoryBase<SysTrackInfo, SysTrackInfoModel>
    {
        Task<TrackingShipmentViewModel> TrackShipmentProgress(TrackingShipmentCriteria criteria);
        bool CheckExistShipment(TrackingShipmentCriteria criteria);
    }
}
