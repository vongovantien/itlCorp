using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ICatCustomerPlaceService: IRepositoryBase<CatCustomerPlace, CatCustomerPlaceModel>
    {
        string ToString();
        List<vw_customerPlace> CustomerPlace();
    }
}