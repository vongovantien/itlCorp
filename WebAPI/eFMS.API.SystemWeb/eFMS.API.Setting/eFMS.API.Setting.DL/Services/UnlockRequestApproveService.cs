using AutoMapper;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class UnlockRequestApproveService : RepositoryBase<SetUnlockRequestApprove, SetUnlockRequestApproveModel>, IUnlockRequestApproveService
    {
        private readonly ICurrentUser currentUser;
        readonly IUserBaseService userBaseService;
        readonly IContextBase<SetUnlockRequest> unlockRequestRepo;
        public UnlockRequestApproveService(
            IContextBase<SetUnlockRequestApprove> repository,
            IMapper mapper,
            ICurrentUser user,
            IUserBaseService userBase,
            IContextBase<SetUnlockRequest> unlockRequest) : base(repository, mapper)
        {
            currentUser = user;
            userBaseService = userBase;
            unlockRequestRepo = unlockRequest;
        }

        public SetUnlockRequestApproveModel GetInfoApproveUnlockRequest(Guid id)
        {
            var userCurrent = currentUser.UserID;
            var unlockApprove = DataContext.Get(x => x.UnlockRequestId == id && x.IsDeny == false).FirstOrDefault();
            
            var unlockApproveModel = new SetUnlockRequestApproveModel();

            if (unlockApprove != null)
            {
                unlockApproveModel = mapper.Map<SetUnlockRequestApproveModel>(unlockApprove);

                unlockApproveModel.IsApproved = false; ///
                unlockApproveModel.LeaderName = userBaseService.GetEmployeeByUserId(unlockApproveModel.Leader)?.EmployeeNameVn;
                unlockApproveModel.ManagerName = userBaseService.GetEmployeeByUserId(unlockApproveModel.Manager)?.EmployeeNameVn;
                unlockApproveModel.AccountantName = userBaseService.GetEmployeeByUserId(unlockApproveModel.Accountant)?.EmployeeNameVn;
                unlockApproveModel.BUHeadName = userBaseService.GetEmployeeByUserId(unlockApproveModel.Buhead)?.EmployeeNameVn;
                unlockApproveModel.StatusApproval = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault()?.StatusApproval;
                unlockApproveModel.NumOfDeny = DataContext.Get(x => x.UnlockRequestId == id && x.IsDeny == true && x.Comment != "RECALL").Select(s => s.Id).Count();
            }
            else
            {
                //Mặc định nếu chưa send request thì gán IsApproved bằng true (nhằm để disable button Approve & Deny)
                unlockApproveModel.IsApproved = true;
            }
            return unlockApproveModel;
        }

        public List<DeniedUnlockRequestResult> GetHistoryDenied(Guid id)
        {
            var approves = DataContext.Get(x => x.UnlockRequestId == id && x.IsDeny == true && x.Comment != "RECALL").ToList();
            var data = new List<DeniedUnlockRequestResult>();
            int i = 1;
            foreach (var approve in approves)
            {
                var item = new DeniedUnlockRequestResult();
                item.No = i;
                item.NameAndTimeDeny = userBaseService.GetEmployeeByUserId(approve.UserModified)?.EmployeeNameVn + " " + approve.DatetimeModified?.ToString("dd/MM/yyyy HH:mm");
                item.LevelApprove = approve.LevelApprove;
                item.Comment = approve.Comment;
                i = i + 1;
            }
            return data;
        }
    }
}
