using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AutoMapper;
using ITL.NetCore.Connection.EF;

using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatVehicleTypeService : RepositoryBase<CatVehicleType, CatVehicleTypeModel>, ICatVehicleTypeService
    {
        public CatVehicleTypeService(IContextBase<CatVehicleType> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        
    }
}
