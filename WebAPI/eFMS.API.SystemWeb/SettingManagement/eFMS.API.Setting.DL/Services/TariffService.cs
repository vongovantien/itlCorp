using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class TariffService : RepositoryBase<SetTariff, SetTariffModel>, ITariffService
    {
        private readonly ICurrentUser currentUser;
        public TariffService(IContextBase<SetTariff> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }   


    }
}
