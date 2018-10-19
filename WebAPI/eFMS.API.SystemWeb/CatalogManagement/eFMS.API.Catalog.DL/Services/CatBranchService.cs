using AutoMapper;
using eFMS.API.Catalog.DL.IService;
using eFMS.API.Catalog.DL.Models;
using eFMS.API.Catalog.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalog.DL.Services
{
    public class CatBranchService : RepositoryBase<CatBranch, CatBranchModel>, ICatBranchService
    {
        public CatBranchService(IContextBase<CatBranch> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
