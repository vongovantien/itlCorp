using System;
using System.Linq;
using System.Linq.Expressions;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.IService
{
    public interface ICatSaleResourceService : IRepositoryBase<CatSaleResource, CatSaleResourceModel>
    {
    }
}
