using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class TariffService : RepositoryBase<Tariff, TariffModel>, ITariffService
    {
        public TariffService(IContextBase<Tariff> repository, IMapper mapper) : base(repository, mapper)
        {
        }   


    }
}
