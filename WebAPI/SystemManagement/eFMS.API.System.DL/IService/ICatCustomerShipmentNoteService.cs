using ITL.NetCore.Connection.BL;
using System.Linq;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ICatCustomerShipmentNoteService: IRepositoryBase<CatCustomerShipmentNote, CatCustomerShipmentNoteModel>
    {
        string ToString();
        IQueryable<CatCustomerShipmentNoteModel> GetFollowCustomer(string CustomerID);
    }
}