using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.System.DL.Services
{
    public class SysSettingFlowService: RepositoryBase<SysSettingFlow, SysSettingFlowModel>, ISysSettingFlowService
    {
        private readonly ICurrentUser currentUser;
        public SysSettingFlowService(
            IMapper mapper,
            IContextBase<SysSettingFlow> repository,
            ICurrentUser ICurrentUser
           ) : base(repository, mapper)
        {
            currentUser = ICurrentUser;
        }


        public List<SysSettingFlowModel> GetByOfficeId(Guid officeId)
        {
            List<SysSettingFlowModel> resultData = new List<SysSettingFlowModel>();

            IQueryable<SysSettingFlow> data = DataContext.Get(x => x.OfficeId == officeId);

            if(data == null)
            {
                return new List<SysSettingFlowModel>();
            }
            foreach (var item in data)
            {
                SysSettingFlowModel settingFlow = mapper.Map<SysSettingFlowModel>(item);
                resultData.Add(settingFlow);
            }
            return resultData;
        }

        public HandleState UpdateSettingFlow(List<SysSettingFlowModel> list, Guid OfficeId)
        {
            HandleState hs = new HandleState();

            if(list.Count() == 0)
            {
                return hs;
            }

            IQueryable<SysSettingFlow> data = DataContext.Get(x => x.OfficeId == OfficeId);
            if(data == null)
            {
                foreach (SysSettingFlowModel item in list)
                {
                    item.OfficeId = OfficeId;
                    hs = DataContext.Add(item, false);
                }
                DataContext.SubmitChanges();
            }
            else
            {
                foreach (SysSettingFlowModel item in list)
                {
                    item.OfficeId = OfficeId;
                    hs = DataContext.Update(item, x => x.OfficeId == OfficeId,false);
                }
                DataContext.SubmitChanges();
            }
            return hs;
        }
    }
}
