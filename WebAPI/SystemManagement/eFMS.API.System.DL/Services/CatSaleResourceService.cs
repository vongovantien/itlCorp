using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ITL.NetCore.Connection.EF;

using SystemManagement.DL.IService;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class CatSaleResourceService : RepositoryBase<CatSaleResource, CatSaleResourceModel>, ICatSaleResourceService
    {
        private readonly IContextBase<CatSaleResource> _repository;
        private readonly IMapper _mapper;
        public CatSaleResourceService(IContextBase<CatSaleResource> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}
