using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;

namespace eFMS.API.System.DL.Services
{
    public class SysEmployeeService : RepositoryBase<SysEmployee, SysEmployeeModel>, ISysEmployeeService
    {
        private readonly IDistributedCache cache;

        public SysEmployeeService(IContextBase<SysEmployee> repository, IMapper mapper, IDistributedCache distributedCache) : base(repository, mapper)
        {
            //currentUser = user;
            cache = distributedCache;

        }

        public HandleState Insert(SysEmployeeModel sysEmployeeModel)
        {
            try
            {
                return DataContext.Add(sysEmployeeModel);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }

        }

        public HandleState Update(SysEmployeeModel model)
        {
            try
            {
                //var entity = mapper.Map<SysEmployee>(model);
                if (model.Active == true)
                {
                    model.InactiveOn = DateTime.Now;
                }
                var hs = DataContext.Update(model, x => x.Id == model.Id);
                if (hs.Success)
                {
                    cache.Remove(Templates.SysBranch.NameCaching.ListName);
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
