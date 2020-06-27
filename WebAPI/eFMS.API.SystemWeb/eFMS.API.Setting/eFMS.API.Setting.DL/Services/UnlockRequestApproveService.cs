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
                                if (infoLevelApprove.Role == "Auto")
                                {
                                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                    unlockApprove.LeaderApr = userCurrent;
                                    unlockApprove.LeaderAprDate = DateTime.Now;
                                    unlockApprove.LevelApprove = "Leader";
                                }
                            }
                        }

                        var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        if (managerLevel.Role == "Auto" || managerLevel.Role == "Approval")
                        {
                            _manager = managerLevel.UserId;
                            if (string.IsNullOrEmpty(_manager)) return new HandleState("Not found manager");
                            if (managerLevel.Role == "Auto" && (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role != "Approval"))
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED;
                                unlockApprove.ManagerApr = userCurrent;
                                unlockApprove.ManagerAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "Manager";
                            }
                        }

                        var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        if (accountantLevel.Role == "Auto" || accountantLevel.Role == "Approval")
                        {
                            _accountant = accountantLevel.UserId;
                            if (string.IsNullOrEmpty(_accountant)) return new HandleState("Not found accountant");
                            if (accountantLevel.Role == "Auto" 
                                && managerLevel.Role != "Approval"
                                && (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role != "Approval"))
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                unlockApprove.AccountantApr = userCurrent;
                                unlockApprove.AccountantAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "Accountant";
                            }
                        }

                        var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        if (buHeadLevel.Role == "Auto" || buHeadLevel.Role == "Approval")
                        {
                            _bhHead = buHeadLevel.UserId;
                            if (string.IsNullOrEmpty(_bhHead)) return new HandleState("Not found BOD");
                            if (buHeadLevel.Role == "Auto" 
                                && accountantLevel.Role != "Approval" 
                                && managerLevel.Role != "Approval" 
                                && (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role != "Approval"))
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                unlockApprove.BuheadApr = userCurrent;
                                unlockApprove.BuheadAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "BOD";
                            }
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
                    result.UserDeputies = new List<string>();
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfManager)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "Manager";
                    result.Role = "Approval";
                    result.UserId = userManager;
                    result.UserDeputies = new List<string>();
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
                    result.UserDeputies = new List<string>();
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfAccountant)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "Accountant";
                    result.Role = "Approval";
                    result.UserId = userAccountant;
                    result.UserDeputies = new List<string>();
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
                    result.UserDeputies = new List<string>();
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Approval":
                    result.LevelApprove = "BOD";
                    result.Role = "Approval";
                    result.UserId = userBuHead;
                    result.UserDeputies = new List<string>();
                    result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
                    result.EmailDeputies = new List<string>();
                    break;
                case "Special":
                    result.LevelApprove = "BOD";
                    result.Role = "Special";
                    result.UserId = userBuHead;
                    result.UserDeputies = new List<string>();
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

        #region -- APPROVE, DENY, CANCEL REQUEST --
        public HandleState UpdateApproval(Guid id)
        {
            var userCurrent = currentUser.UserID;
            var emailUserAprNext = string.Empty;
            List<string> emailUserDeputies = new List<string>();

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var unlockRequest = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault();

                    if (unlockRequest == null) return new HandleState("Not found unlock request");

                    var approve = DataContext.Get(x => x.UnlockRequestId == unlockRequest.Id && x.IsDeny == false).FirstOrDefault();

                    if (approve == null)
                    {
                        return new HandleState("Not found unlock request approval");
                    }

                    var infoLevelApprove = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

                    if (infoLevelApprove.Role != "None")
                    {
                        if (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role == "Approval")
                        {
                            if (string.IsNullOrEmpty(approve.LeaderApr))
                            {
                                if ((userCurrent == infoLevelApprove.UserId || infoLevelApprove.UserDeputies.Contains(userCurrent)) && !string.IsNullOrEmpty(unlockRequest.RequestUser))
                                {
                                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                    approve.LeaderApr = userCurrent;
                                    approve.LeaderAprDate = DateTime.Now;
                                    approve.LevelApprove = "Leader";
                                    emailUserAprNext = managerLevel.EmailUser;
                                    emailUserDeputies = managerLevel.EmailDeputies;
                                    if (managerLevel.Role == "Auto")
                                    {
                                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED;
                                        approve.ManagerApr = managerLevel.UserId;
                                        approve.ManagerAprDate = DateTime.Now;
                                        approve.LevelApprove = "Manager";
                                        emailUserAprNext = accountantLevel.EmailUser;
                                        emailUserDeputies = accountantLevel.EmailDeputies;
                                        if (accountantLevel.Role == "Auto")
                                        {
                                            unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                            approve.AccountantApr = accountantLevel.UserId;
                                            approve.AccountantAprDate = DateTime.Now;
                                            approve.LevelApprove = "Accountant";
                                            emailUserAprNext = buHeadLevel.EmailUser;
                                            emailUserDeputies = buHeadLevel.EmailDeputies;
                                            if (buHeadLevel.Role == "Auto")
                                            {
                                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                                approve.BuheadApr = buHeadLevel.UserId;
                                                approve.BuheadAprDate = DateTime.Now;
                                                approve.LevelApprove = "BOD";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    return new HandleState("Not allow approve");
                                }
                            }
                            else
                            {
                                return new HandleState("The leader has not approved it yet");
                            }
                        }

                        if (managerLevel.Role == "Approval")
                        {
                            if (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role == "Approval" && string.IsNullOrEmpty(approve.LeaderApr))
                            {
                                return new HandleState("Leader not approve");
                            }
                            if (string.IsNullOrEmpty(approve.ManagerApr))
                            {
                                if (userCurrent == managerLevel.UserId || managerLevel.UserDeputies.Contains(userCurrent))
                                {
                                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED;
                                    approve.ManagerApr = userCurrent;
                                    approve.ManagerAprDate = DateTime.Now;
                                    approve.LevelApprove = "Manager";
                                    emailUserAprNext = accountantLevel.EmailUser;
                                    emailUserDeputies = accountantLevel.EmailDeputies;
                                    if (accountantLevel.Role == "Auto")
                                    {
                                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                        approve.AccountantApr = accountantLevel.UserId;
                                        approve.AccountantAprDate = DateTime.Now;
                                        approve.LevelApprove = "Accountant";
                                        emailUserAprNext = buHeadLevel.EmailUser;
                                        emailUserDeputies = buHeadLevel.EmailDeputies;
                                        if (buHeadLevel.Role == "Auto")
                                        {
                                            unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                            approve.BuheadApr = buHeadLevel.UserId;
                                            approve.BuheadAprDate = DateTime.Now;
                                            approve.LevelApprove = "BOD";
                                        }
                                    }
                                }
                                else
                                {
                                    return new HandleState("Not allow approve");
                                }
                            }
                            else
                            {
                                return new HandleState("Manager approved");
                            }
                        }

                        if (accountantLevel.Role == "Approval")
                        {
                            if (managerLevel.Role == "Approval" && string.IsNullOrEmpty(approve.ManagerApr))
                            {
                                return new HandleState("The manager has not approved it yet");
                            }
                            if (string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                if (userCurrent == accountantLevel.UserId || accountantLevel.UserDeputies.Contains(userCurrent))
                                {
                                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                    approve.AccountantApr = userCurrent;
                                    approve.AccountantAprDate = DateTime.Now;
                                    approve.LevelApprove = "Accountant";
                                    emailUserAprNext = buHeadLevel.EmailUser;
                                    emailUserDeputies = buHeadLevel.EmailDeputies;
                                    if (buHeadLevel.Role == "Auto")
                                    {
                                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                        approve.BuheadApr = buHeadLevel.UserId;
                                        approve.BuheadAprDate = DateTime.Now;
                                        approve.LevelApprove = "BOD";
                                    }
                                }
                                else
                                {
                                    return new HandleState("Not allow approve");
                                }
                            }
                            else
                            {
                                return new HandleState("Accountant approved");
                            }
                        }

                        if (buHeadLevel.Role == "Approval")
                        {
                            if (accountantLevel.Role == "Approval" && string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                return new HandleState("The accountant has not approved it yet");
                            }
                            if (string.IsNullOrEmpty(approve.BuheadApr))
                            {
                                if (userCurrent == buHeadLevel.UserId || buHeadLevel.UserDeputies.Contains(userCurrent))
                                {
                                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                    approve.BuheadApr = userCurrent;
                                    approve.BuheadAprDate = DateTime.Now;
                                    approve.LevelApprove = "BOD";

                                    //Send Mail Approved
                                }
                                else
                                {
                                    return new HandleState("Not allow approve");
                                }
                            }
                            else
                            {
                                return new HandleState("BOD approved");
                            }
                        }
                    }
                    else
                    {
                        //Cập nhật Status Approval là Done cho Unlock Request
                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                    }

                    var sendMailApproved = true;
                    var sendMailSuggest = true;
                    if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                    {
                        //Send Mail Approved

                    }
                    else
                    {
                        //Send Mail Suggest

                    }

                    if (!sendMailSuggest)
                    {
                        return new HandleState("Send mail suggest Approval failed");
                    }
                    if (!sendMailApproved)
                    {
                        return new HandleState("Send mail Approved Approval failed");
                    }

                    unlockRequest.UserModified = approve.UserModified = userCurrent;
                    unlockRequest.DatetimeModified = approve.DatetimeModified = DateTime.Now;

                    var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
                    var hsUpdateApprove = DataContext.Update(approve, x => x.Id == approve.Id);

                    unlockRequestRepo.SubmitChanges();
                    trans.Commit();
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

        public HandleState DeniedApprove(Guid id, string comment)
        {
            var userCurrent = currentUser.UserID;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var unlockRequest = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault();
                    if (unlockRequest == null) return new HandleState("Not found unlock request");
                    var approve = DataContext.Get(x => x.UnlockRequestId == unlockRequest.Id && x.IsDeny == false).FirstOrDefault();

                    if (approve == null)
                    {
                        return new HandleState("Not found unlock request approval");
                    }

                    if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DENIED)
                    {
                        return new HandleState("Unlock request has been denied");
                    }
                    var infoLevelApprove = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

                    if (infoLevelApprove.Role != "None")
                    {
                        if (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role == "Approval")
                        {
                            if ((userCurrent == infoLevelApprove.UserId || infoLevelApprove.UserDeputies.Contains(userCurrent)) && !string.IsNullOrEmpty(unlockRequest.RequestUser))
                            {
                                approve.LeaderApr = userCurrent;
                                approve.LeaderAprDate = DateTime.Now;
                                approve.LevelApprove = "Leader";
                            }
                            else
                            {
                                return new HandleState("Not allow deny");
                            }
                        }

                        if (managerLevel.Role == "Approval")
                        {
                            if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Not allow deny. Unlock request has been approved");
                            }

                            if (infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role == "Approval" && string.IsNullOrEmpty(approve.LeaderApr))
                            {
                                return new HandleState("Leader not approve");
                            }

                            if (userCurrent == managerLevel.UserId || managerLevel.UserDeputies.Contains(userCurrent))
                            {
                                approve.ManagerApr = userCurrent;
                                approve.ManagerAprDate = DateTime.Now;
                                approve.LevelApprove = "Manager";
                            }
                            else
                            {
                                return new HandleState("Not allow deny");
                            }
                        }

                        if (accountantLevel.Role == "Approval")
                        {
                            if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                            {
                                return new HandleState("Not allow deny. Unlock request has been approved");
                            }

                            if (managerLevel.Role == "Approval" && string.IsNullOrEmpty(approve.ManagerApr))
                            {
                                return new HandleState("The manager has not approved it yet");
                            }

                            if (userCurrent == accountantLevel.UserId || accountantLevel.UserDeputies.Contains(userCurrent))
                            {
                                approve.AccountantApr = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                                approve.LevelApprove = "Accountant";
                            }
                            else
                            {
                                return new HandleState("Not allow deny");
                            }
                        }

                        if (buHeadLevel.Role == "Approval")
                        {
                            if (accountantLevel.Role == "Approval" && string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                return new HandleState("The accountant has not approved it yet");
                            }

                            if (userCurrent == buHeadLevel.UserId || buHeadLevel.UserDeputies.Contains(userCurrent))
                            {
                                approve.BuheadApr = userCurrent;
                                approve.BuheadAprDate = DateTime.Now;
                                approve.LevelApprove = "BOD";
                            }
                            else
                            {
                                return new HandleState("Not allow deny");
                            }
                        }
                    }
                    else
                    {
                        //Cập nhật Status Approval là Done cho Unlock Request
                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;

                    }

                    var sendMailDeny = true;
                    if (!sendMailDeny)
                    {
                        return new HandleState("Send mail denied failed");
                    }

                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DENIED;
                    unlockRequest.RequestUser = null;
                    unlockRequest.RequestDate = null;
                    approve.IsDeny = true;
                    approve.Comment = comment;
                    unlockRequest.UserModified = approve.UserModified = userCurrent;
                    unlockRequest.DatetimeModified = approve.DatetimeModified = DateTime.Now;

                    var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
                    var hsUpdateApprove = DataContext.Update(approve, x => x.Id == approve.Id);

                    unlockRequestRepo.SubmitChanges();
                    trans.Commit();
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

        public HandleState CancelRequest(Guid id)
        {
            var userCurrent = currentUser.UserID;
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var unlockRequest = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault();
                    if (unlockRequest == null)
                    {
                        return new HandleState("Not found unlock request");
                    }
                    else
                    {
                        if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DENIED
                            || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_NEW)
                        {
                            return new HandleState("Unlock request not yet send the request");
                        }
                        if (unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED
                            && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW
                            && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)
                        {
                            return new HandleState("Unlock request approving");
                        }

                        var approve = DataContext.Get(x => x.UnlockRequestId == unlockRequest.Id && x.IsDeny == false).FirstOrDefault();
                        //Cập nhật lại approve
                        if (approve != null)
                        {
                            approve.UserModified = userCurrent;
                            approve.DatetimeModified = DateTime.Now;
                            approve.Comment = "RECALL";
                            approve.IsDeny = true;
                            var hsUpdateApprove = DataContext.Update(approve, x => x.Id == approve.Id, false);
                        }

                        //Cập nhật lại advance status của Unlock request
                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_NEW;
                        unlockRequest.UserModified = userCurrent;
                        unlockRequest.DatetimeModified = DateTime.Now;
                        var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
                        unlockRequestRepo.SubmitChanges();
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
        #endregion -- APPROVE, DENY, CANCEL REQUEST --

        public bool CheckUserInApprove(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve)
        {
            var isApproved = false;

            var infoLevelApprove = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
            // 1 user vừa có thể là Requester, Manager Dept, Accountant Dept nên khi check Approved cần phải dựa vào group
            // Group 11 chính là group Manager

            if (userCurrent.GroupId != SettingConstants.SpecialGroup
                && userCurrent.UserID == unlockRequest.RequestUser) //Requester
            {
                isApproved = true;
                if (unlockRequest.RequestDate == null)
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId != SettingConstants.SpecialGroup 
                && (
                    (infoLevelApprove.UserDeputies.Contains(userCurrent.UserID) && infoLevelApprove.LevelApprove == "Leader") 
                || userCurrent.UserID == approve.Leader)) //Leader
            {
                isApproved = true;
                if (approve.LeaderAprDate == null && infoLevelApprove.LevelApprove == "Leader" && infoLevelApprove.Role != "None")
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId == SettingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId) == false
                && (userCurrent.UserID == approve.Manager
                    || userCurrent.UserID == approve.ManagerApr
                    || managerLevel.UserDeputies.Contains(currentUser.UserID))) //Dept Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = unlockRequestRepo.Get(x => x.Id == approve.UnlockRequestId && (x.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED)).Any();
                if (approve.ManagerAprDate == null && isDeptWaitingApprove && managerLevel.Role != "None")
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.GroupId == SettingConstants.SpecialGroup
                && userBaseService.CheckIsAccountantDept(userCurrent.DepartmentId)
                && (userCurrent.UserID == approve.Accountant
                    || userCurrent.UserID == approve.AccountantApr
                    || accountantLevel.UserDeputies.Contains(currentUser.UserID))) //Accountant Manager
            {
                isApproved = true;
                var isDeptWaitingApprove = unlockRequestRepo.Get(x => x.Id == approve.UnlockRequestId && (x.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW && x.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED && x.StatusApproval != SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL)).Any();
                if (approve.AccountantAprDate == null && isDeptWaitingApprove && accountantLevel.Role != "None")
                {
                    isApproved = false;
                }
            }
            else if (userCurrent.UserID == approve.Buhead 
                || userCurrent.UserID == approve.BuheadApr 
                || buHeadLevel.UserDeputies.Contains(userCurrent.UserID)) //BUHead
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approve.BuheadApr) && approve.BuheadAprDate == null && buHeadLevel.Role != "None")
                {
                    isApproved = false;
                }
            }
            else
            {
                //Đây là trường hợp các User không thuộc Approve Advance
                isApproved = true;
            }
            return isApproved;
        }
    }
}
