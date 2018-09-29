using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.IService;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatShipmentTypeService : RepositoryBase<CatShipmentType, CatShipmentTypeModel>, ICatShipmentTypeService
    {
        public CatShipmentTypeService(IContextBase<CatShipmentType> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
