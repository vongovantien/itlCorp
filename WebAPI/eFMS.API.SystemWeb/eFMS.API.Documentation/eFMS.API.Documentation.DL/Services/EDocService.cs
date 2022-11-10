using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class EDocService : RepositoryBase<SysImageDetail, SysImageDetail>, IEDocService
    {
        public EDocService(IContextBase<SysImageDetail> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<HandleState> DeleteEdocByHBLId(Guid HBLId)
        {
            var result = await DataContext.DeleteAsync(x => x.Hblid == HBLId);
            if (!result.Success)
            {
                return new HandleState("Can't Delete Edoc");
            }
            return new HandleState(); ;
        }
    }
}
