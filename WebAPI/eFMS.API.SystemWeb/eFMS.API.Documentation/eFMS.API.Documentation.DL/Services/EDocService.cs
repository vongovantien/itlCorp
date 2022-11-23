using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class EDocService : RepositoryBase<SysImageDetail, SysImageDetail>, IEDocService
    {
        readonly IContextBase<SysImage> _sysImageRepo;
        public EDocService(
            IContextBase<SysImageDetail> repository,
            IContextBase<SysImage> sysImageRepo,
            IMapper mapper
            ) : base(repository, mapper)
        {
            _sysImageRepo = sysImageRepo;
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

        public async Task<HandleState> DeleteEdocByJobId(Guid JobID)
        {
            var images =await _sysImageRepo.Get(x => x.ObjectId == JobID.ToString() && x.Folder == "Shipment").ToListAsync();
            images.ForEach(async x =>
            {
                await _sysImageRepo.DeleteAsync(z => z.Id == x.Id);
            });
            var result = await DataContext.DeleteAsync(x => x.JobId == JobID);
            if (!result.Success)
            {
                return new HandleState("Can't Delete Edoc");
            }
            return new HandleState(); ;
        }
    }
}
