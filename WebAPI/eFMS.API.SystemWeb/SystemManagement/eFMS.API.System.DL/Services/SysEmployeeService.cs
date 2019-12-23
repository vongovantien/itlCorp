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
using System.Linq;

namespace eFMS.API.System.DL.Services
{
    public class SysEmployeeService : RepositoryBase<SysEmployee, SysEmployeeModel>, ISysEmployeeService
    {
        private IContextBase<SysUser> userRepository;

        public SysEmployeeService(IContextBase<SysEmployee> repository, IContextBase<SysUser> userRepo, IMapper mapper, IDistributedCache distributedCache) : base(repository, mapper)
        {
            userRepository = userRepo;
        }

        public SysEmployeeModel GetByUser(string userId)
        {
            var user = userRepository.Get(x => x.Id == userId)?.FirstOrDefault();
            if (user == null) return null;
            var employee = Get(x => x.Id == user.EmployeeId)?.FirstOrDefault();
            return employee;
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
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
