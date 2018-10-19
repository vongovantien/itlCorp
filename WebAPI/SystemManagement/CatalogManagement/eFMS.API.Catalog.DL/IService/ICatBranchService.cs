using eFMS.API.Catalog.DL.Models;
using eFMS.API.Catalog.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalog.DL.IService
{
    public interface ICatBranchService : IRepositoryBase<CatBranch, CatBranchModel>
    {
    }
}
