using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsStageAssignedService : RepositoryBase<OpsStageAssigned, CsStageAssignedModel>, ICsStageAssignedService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<OpsTransaction> opsTransRepository;
        private readonly IContextBase<CsTransaction> csTransactionReporsitory;
        private readonly IContextBase<SysUser> userRepository;
        public CsStageAssignedService(
            ICurrentUser user,
            IContextBase<OpsTransaction> opsTransRepo,
            IContextBase<OpsStageAssigned> repository, IMapper mapper) : base(repository, mapper)
        {
            currentUser = user;
            opsTransRepository = opsTransRepo;
        }
        public HandleState AddNewStageAssigned(CsStageAssignedModel model)
        {
            var assignedItem = mapper.Map<OpsStageAssigned>(model);
            DataContext.Add(assignedItem, false);

            HandleState hs = DataContext.SubmitChanges();
            return hs;
        }
    }
}
