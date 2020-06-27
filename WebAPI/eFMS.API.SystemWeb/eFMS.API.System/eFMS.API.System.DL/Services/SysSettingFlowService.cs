using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Models;
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
        private readonly IContextBase<SetLockingDateShipment> setLockingDateShipmentRepository;

        public SysSettingFlowService(
            IMapper mapper,
            IContextBase<SysSettingFlow> repository,
            IContextBase<SetLockingDateShipment> setLockingDateShipmentRepo,
            ICurrentUser ICurrentUser
           ) : base(repository, mapper)
        {
            currentUser = ICurrentUser;
            setLockingDateShipmentRepository = setLockingDateShipmentRepo;
        }


        public SysSettingFlowViewModel GetByOfficeId(Guid officeId)
        {
            SysSettingFlowViewModel resultData = new SysSettingFlowViewModel();

            IQueryable<SysSettingFlow> data = DataContext.Get(x => x.OfficeId == officeId);

            if(data == null)
            {
                return null;
            }

            resultData.Approvals = data.Where(x => x.Flow == "Approval").ToList();
            resultData.Unlocks = data.Where(x => x.Flow == "Unlock").ToList();

            List<SetLockingDateShipment> dataLockingDateShipment = setLockingDateShipmentRepository.Where(x => x.OfficeId == officeId).ToList();

            resultData.LockingDateShipment = dataLockingDateShipment;

            return resultData;
        }

        public HandleState UpdateSettingFlow(SysSettingFlowEditModel model)
        {
            HandleState hs = new HandleState();

            if(model.ApprovePayments.Count() == 0 || model.UnlockShipments.Count() == 0)
            {
                return hs;
            }

            IQueryable<SysSettingFlow> data = DataContext.Get(x => x.OfficeId == model.OfficeId);
            IQueryable<SetLockingDateShipment> lockingDateShipments = setLockingDateShipmentRepository.Get(x => x.OfficeId == model.OfficeId);

            List<SysSettingFlowModel> list = new List<SysSettingFlowModel> { };
            foreach (var item in model.ApprovePayments)
            {
                list.Add(item);
            }
            foreach (var item in model.UnlockShipments)
            {
                list.Add(item);
            }

            if (data.Count() == 0)
            {
                foreach (SysSettingFlowModel item in list)
                {
                    item.OfficeId = model.OfficeId;
                    item.UserCreated = item.UserModified = currentUser.UserID;
                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                    item.Id = Guid.NewGuid();

                    hs = DataContext.Add(item, false);
                }
            }
            else
            {
                foreach (SysSettingFlowModel item in list)
                {
                    item.OfficeId = model.OfficeId;
                    item.UserModified = currentUser.UserID;
                    item.DatetimeModified = DateTime.Now;

                    hs = DataContext.Update(item, x => x.Id == item.Id, false);
                }
            }
            DataContext.SubmitChanges();

            if (lockingDateShipments.Count() == 0)
            {
                foreach (SetLockingDateShipment item in model.LockShipmentDate)
                {
                    item.UserModified = item.UserCreated = currentUser.UserID;
                    item.DatetimeModified = item.DatetimeCreated = DateTime.Now;
                    item.OfficeId = model.OfficeId;

                    hs = setLockingDateShipmentRepository.Add(item, false);
                }
            }
            else
            {
                foreach (SetLockingDateShipment item in model.LockShipmentDate)
                {
                    item.UserModified  = currentUser.UserID;
                    item.DatetimeModified = DateTime.Now;
                    item.OfficeId = model.OfficeId;

                    hs = setLockingDateShipmentRepository.Update(item, x => x.Id == item.Id, false);
                }
            }

            setLockingDateShipmentRepository.SubmitChanges();

            return hs;
        }
    }
}
