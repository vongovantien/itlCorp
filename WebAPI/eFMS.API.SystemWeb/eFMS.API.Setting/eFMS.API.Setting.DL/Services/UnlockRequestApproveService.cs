using AutoMapper;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
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
            var unlockRequest = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault();
            if (unlockRequest != null)
            {
                unlockRequest.UnlockType = unlockRequest.UnlockType == "Change Service Date" ? "Shipment" : unlockRequest.UnlockType;
            }

            SetUnlockRequestApproveModel unlockApproveModel = new SetUnlockRequestApproveModel();

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
                unlockApproveModel.IsShowLeader = userBaseService.GetRoleByLevel("Leader", unlockRequest?.UnlockType, unlockRequest.OfficeId) != "None" ? true : false;
                unlockApproveModel.IsShowManager = userBaseService.GetRoleByLevel("Manager", unlockRequest?.UnlockType, unlockRequest.OfficeId) != "None" ? true : false;
                unlockApproveModel.IsShowAccountant = userBaseService.GetRoleByLevel("Accountant", unlockRequest?.UnlockType, unlockRequest.OfficeId) != "None" ? true : false;
                unlockApproveModel.IsShowBuHead = userBaseService.GetRoleByLevel("BOD", unlockRequest?.UnlockType, unlockRequest.OfficeId) != "None" ? true : false;
            }
            else
            {
                unlockApproveModel.StatusApproval = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault()?.StatusApproval;
                unlockApproveModel.NumOfDeny = DataContext.Get(x => x.UnlockRequestId == id && x.IsDeny == true && x.Comment != "RECALL").Select(s => s.Id).Count();
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
                item.NameAndTimeDeny = userBaseService.GetEmployeeByUserId(approve.UserModified)?.EmployeeNameVn + "\n" + approve.DatetimeModified?.ToString("dd/MM/yyyy HH:mm");
                item.LevelApprove = approve.LevelApprove;
                item.Comment = approve.Comment;
                data.Add(item);
                i = i + 1;
            }
            return data;
        }

        public HandleState InsertOrUpdateApproval(SetUnlockRequestApproveModel approve)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var unlockApprove = mapper.Map<SetUnlockRequestApprove>(approve);

                var unlockRequest = unlockRequestRepo.Get(x => x.Id == approve.UnlockRequestId).FirstOrDefault();
                if (unlockRequest != null) unlockRequest.UnlockType = unlockRequest.UnlockType == "Change Service Date" ? "Shipment" : unlockRequest.UnlockType;

                if (approve.UnlockRequestId == Guid.Empty)
                {                    
                    if (unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                        && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED
                        && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DONE
                        && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                    {
                        return new HandleState("Awaiting approval");
                    }
                }

                // Check existing Settling Flow
                var settingFlow = userBaseService.GetSettingFlowUnlock(unlockRequest.UnlockType, unlockRequest.OfficeId);
                if (settingFlow == null)
                {
                    return new HandleState("No setting flow yet");
                }

                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        string _leader = null;
                        string _manager = null;
                        string _accountant = null;
                        string _bhHead = null;

                        var infoLevelApprove = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);

                        if (infoLevelApprove.Role == "None")
                        {
                            //Cập nhật Status Approval là Done cho Unlock Request
                            unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                            var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
                        }
                        else if (infoLevelApprove.Role == "Auto" || infoLevelApprove.Role == "Approval")
                        {
                            if (infoLevelApprove.LevelApprove == "Leader")
                            {
                                _leader = infoLevelApprove.UserId;
                                if (string.IsNullOrEmpty(_leader)) return new HandleState("Not found leader");
                            }
                        }

                        var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        if (managerLevel.Role == "Auto" || managerLevel.Role == "Approval")
                        {
                            _manager = managerLevel.UserId;
                            if (string.IsNullOrEmpty(_manager)) return new HandleState("Not found manager");
                        }

                        var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        if (accountantLevel.Role == "Auto" || accountantLevel.Role == "Approval")
                        {
                            _accountant = accountantLevel.UserId;
                            if (string.IsNullOrEmpty(_accountant)) return new HandleState("Not found accountant");
                        }

                        var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        if (buHeadLevel.Role == "Auto" || buHeadLevel.Role == "Approval")
                        {
                            _bhHead = buHeadLevel.UserId;
                            if (string.IsNullOrEmpty(_bhHead)) return new HandleState("Not found BOD");
                        }

                        //var sendMailResult = SendMailSuggestApproval(acctApprove.AdvanceNo, userLeaderOrManager, emailLeaderOrManager, usersDeputy);

                        //if (sendMailResult)
                        {
                            var checkExistsApproveByUnlockRequestId = DataContext.Get(x => x.UnlockRequestId == approve.UnlockRequestId && x.IsDeny == false).FirstOrDefault();
                            if (checkExistsApproveByUnlockRequestId == null) //Insert UnlockRequestApprove
                            {
                                unlockApprove.Id = Guid.NewGuid();
                                unlockApprove.Leader = _leader;
                                unlockApprove.Manager = _manager;
                                unlockApprove.Accountant = _accountant;
                                unlockApprove.Buhead = _bhHead;
                                unlockApprove.UserCreated = unlockApprove.UserModified = userCurrent;
                                unlockApprove.DatetimeCreated = unlockApprove.DatetimeModified = DateTime.Now;
                                unlockApprove.IsDeny = false;
                                var hsAddApprove = DataContext.Add(unlockApprove, false);
                            }
                            else //Update unlockRequest by Id
                            {
                                checkExistsApproveByUnlockRequestId.UserModified = userCurrent;
                                checkExistsApproveByUnlockRequestId.DatetimeModified = DateTime.Now;
                                var hsUpdateApprove = DataContext.Update(checkExistsApproveByUnlockRequestId, x => x.Id == checkExistsApproveByUnlockRequestId.Id);
                            }

                            unlockRequestRepo.SubmitChanges();
                            DataContext.SubmitChanges();
                            trans.Commit();
                        }

                        return new HandleState();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message.ToString());
            }
        }

        #region -- Info Level Approve --
        public InfoLevelApproveResult LeaderLevel(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleLeader = userBaseService.GetRoleByLevel("Leader", type, officeId);
            var userLeader = userBaseService.GetLeaderGroup(companyId, officeId, departmentId, groupId).FirstOrDefault();
            var employeeIdOfLeader = userBaseService.GetEmployeeIdOfUser(userLeader);

            switch (roleLeader)
            {
                case "None":
                    result = ManagerLevel(type, departmentId, officeId, companyId);
                    break;
                case "Auto":                    
                    result.LevelApprove = "Leader";
                    result.Role = "Auto";
                    result.UserId = userLeader;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfLeader)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "Leader";
                    result.Role = "Approval";
                    result.UserId = userLeader;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfLeader)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                default:
                    break;
            }
            return result;
        }

        public InfoLevelApproveResult ManagerLevel(string type, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleManager = userBaseService.GetRoleByLevel("Manager", type, officeId);
            var userManager = userBaseService.GetDeptManager(companyId, officeId, departmentId).FirstOrDefault();
            var employeeIdOfManager = userBaseService.GetEmployeeIdOfUser(userManager);

            switch (roleManager)
            {
                case "None":
                    result = AccountantLevel(type, officeId, companyId);
                    break;
                case "Auto":
                    result.LevelApprove = "Manager";
                    result.Role = "Auto";
                    result.UserId = userManager;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfManager)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "Manager";
                    result.Role = "Approval";
                    result.UserId = userManager;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfManager)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                default:
                    break;
            }
            return result;
        }

        public InfoLevelApproveResult AccountantLevel(string type, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleAccountant = userBaseService.GetRoleByLevel("Accountant", type, officeId);
            var userAccountant = userBaseService.GetAccoutantManager(companyId, officeId).FirstOrDefault();
            var employeeIdOfAccountant = userBaseService.GetEmployeeIdOfUser(userAccountant);

            switch (roleAccountant)
            {
                case "None":
                    result = BuHeadLevel(type, officeId, companyId);
                    break;
                case "Auto":
                    result.LevelApprove = "Accountant";
                    result.Role = "Auto";
                    result.UserId = userAccountant;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfAccountant)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "Accountant";
                    result.Role = "Approval";
                    result.UserId = userAccountant;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfAccountant)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                default:
                    break;
            }
            return result;
        }

        public InfoLevelApproveResult BuHeadLevel(string type, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleBuHead = userBaseService.GetRoleByLevel("BOD", type, officeId);
            var userBuHead = userBaseService.GetBUHead(companyId, officeId).FirstOrDefault();
            var employeeIdOfBuHead = userBaseService.GetEmployeeIdOfUser(userBuHead);

            switch (roleBuHead)
            {
                case "None":
                    result.LevelApprove = "BOD";
                    result.Role = "None";
                    break;
                case "Auto":
                    result.LevelApprove = "BOD";
                    result.Role = "Auto";
                    result.UserId = userBuHead;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "BOD";
                    result.Role = "Approval";
                    result.UserId = userBuHead;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Special":
                    result.LevelApprove = "BOD";
                    result.Role = "Special";
                    result.UserId = userBuHead;
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                default:
                    break;
            }
            return result;
        }
        #endregion -- Info Level Approve --

        #region -- Check Exist --
        public HandleState CheckExistSettingFlow(string type, Guid? officeId)
        {
            type = (type == "Change Service Date") ? "Shipment" : type;
            // Check existing Settling Flow
            var settingFlow = userBaseService.GetSettingFlowUnlock(type, officeId);
            if (settingFlow == null)
            {
                return new HandleState("No setting flow yet");
            }
            return new HandleState();
        }

        public HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var infoLevelApprove = LeaderLevel(type, groupId, departmentId, officeId, companyId);

            if (infoLevelApprove.Role == "Auto" || infoLevelApprove.Role == "Approval")
            {
                if (infoLevelApprove.LevelApprove == "Leader")
                {
                    if (string.IsNullOrEmpty(infoLevelApprove.UserId)) return new HandleState("Not found leader");
                }
            }

            var managerLevel = ManagerLevel(type, departmentId, officeId, companyId);
            if (managerLevel.Role == "Auto" || managerLevel.Role == "Approval")
            {
                if (string.IsNullOrEmpty(managerLevel.UserId)) return new HandleState("Not found manager");
            }

            var accountantLevel = AccountantLevel(type, officeId, companyId);
            if (accountantLevel.Role == "Auto" || accountantLevel.Role == "Approval")
            {
                if (string.IsNullOrEmpty(accountantLevel.UserId)) return new HandleState("Not found accountant");
            }

            var buHeadLevel = BuHeadLevel(type, officeId, companyId);
            if (buHeadLevel.Role == "Auto" || buHeadLevel.Role == "Approval")
            {
                if (string.IsNullOrEmpty(buHeadLevel.UserId)) return new HandleState("Not found BOD");
            }
            return new HandleState();
        }
        #endregion -- Check Exist --
    }
}
