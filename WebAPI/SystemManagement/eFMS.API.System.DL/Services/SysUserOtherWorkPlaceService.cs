using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class SysUserOtherWorkPlaceService : RepositoryBase<SysUserOtherWorkPlace, SysUserOtherWorkPlaceModel>, ISysUserOtherWorkPlaceService
    {
        public SysUserOtherWorkPlaceService(IContextBase<SysUserOtherWorkPlace> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public override string ToString()
        {
            return base.ToString();
        }
        public IQueryable<SysUserOtherWorkPlaceModel> GetFollowUser(string UserId)
        {
            return Get(t => t.UserId.Equals(UserId));
        }
    }
}
