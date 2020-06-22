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
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SetLockingDateShipmentService : RepositoryBase<SetLockingDateShipment, SetLockingDateShipmentModel>, ISetLockingDateShipment
    {
        private readonly ICurrentUser currentUser;

        public SetLockingDateShipmentService(
            IMapper mapper,
            IContextBase<SetLockingDateShipment> repository,
            ICurrentUser ICurrentUser
           ) : base(repository, mapper)
        {
            currentUser = ICurrentUser;
        }
    }
}
