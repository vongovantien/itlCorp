using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatCommodityGroupService : IRepositoryBase<CatCommodityGroup, CatCommodityGroupModel>
    {
    }
}
