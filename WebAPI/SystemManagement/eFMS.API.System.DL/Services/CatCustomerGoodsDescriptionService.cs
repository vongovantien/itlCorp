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
    public class CatCustomerGoodsDescriptionService : RepositoryBase<CatCustomerGoodsDescription, CatCustomerGoodsDescriptionModel>, ICatCustomerGoodsDescriptionService
    {
        public CatCustomerGoodsDescriptionService(IContextBase<CatCustomerGoodsDescription> repository, IMapper mapper) : base(repository, mapper)
        {
        }
        
    }
}
