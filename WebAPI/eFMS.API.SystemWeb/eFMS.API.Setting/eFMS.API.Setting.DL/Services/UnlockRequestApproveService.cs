using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Setting.DL.Services
{
    public class UnlockRequestApproveService : RepositoryBase<SetUnlockRequestApprove, SetUnlockRequestApproveModel>, IUnlockRequestApproveService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> apiUrl;
        readonly IUserBaseService userBaseService;
        readonly IContextBase<SetUnlockRequest> unlockRequestRepo;
        readonly IContextBase<SetUnlockRequestJob> unlockRequestJobRepo;
        readonly IContextBase<OpsTransaction> opsTransactionRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<AcctAdvancePayment> advancePaymentRepo;
        readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        readonly IContextBase<AcctApproveAdvance> approveAdvanceRepo;
        readonly IContextBase<AcctApproveSettlement> approveSettlementRepo;

        public UnlockRequestApproveService(
            IContextBase<SetUnlockRequestApprove> repository,
            IOptions<WebUrl> wUrl,
            IOptions<ApiUrl> aUrl,
            IMapper mapper,
            ICurrentUser user,
            IUserBaseService userBase,
            IContextBase<SetUnlockRequest> unlockRequest,
            IContextBase<SetUnlockRequestJob> unlockRequestJob,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<AcctAdvancePayment> advancePayment,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<AcctApproveAdvance> approveAdvance,
            IContextBase<AcctApproveSettlement> approveSettlement) : base(repository, mapper)
        {
            webUrl = wUrl;
            apiUrl = aUrl;
            currentUser = user;
            userBaseService = userBase;
            unlockRequestRepo = unlockRequest;
            unlockRequestJobRepo = unlockRequestJob;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            advancePaymentRepo = advancePayment;
            settlementPaymentRepo = settlementPayment;
            approveAdvanceRepo = approveAdvance;
            approveSettlementRepo = approveSettlement;
        }

        public SetUnlockRequestApproveModel GetInfoApproveUnlockRequest(Guid id)
        {
            var userCurrent = currentUser.UserID;
            var unlockApprove = DataContext.Get(x => x.UnlockRequestId == id && x.IsDeny == false).FirstOrDefault();
            var unlockRequest = unlockRequestRepo.Get(x => x.Id == id).FirstOrDefault();

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
                unlockApproveModel.IsShowLeader = !string.IsNullOrEmpty(unlockApprove.Leader);
                unlockApproveModel.IsShowManager = !string.IsNullOrEmpty(unlockApprove.Manager);
                unlockApproveModel.IsShowAccountant = !string.IsNullOrEmpty(unlockApprove.Accountant);
                unlockApproveModel.IsShowBuHead = !string.IsNullOrEmpty(unlockApprove.Buhead);
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
                item.NameAndTimeDeny = userBaseService.GetEmployeeByUserId(approve.UserModified)?.EmployeeNameVn + "\r\n" + approve.DatetimeModified?.ToString("dd/MM/yyyy HH:mm");
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

                        var isAllLevelAutoOrNone = CheckAllLevelIsAutoOrNone(unlockRequest.UnlockType, unlockRequest.OfficeId);
                        if (isAllLevelAutoOrNone)
                        {
                            //Cập nhật Status Approval là Done cho Unlock Request
                            unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                            var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
                        }

                        var leaderLevel = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                        var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

                        var userLeaderOrManager = string.Empty;
                        var mailLeaderOrManager = string.Empty;
                        List<string> mailUsersDeputy = new List<string>();

                        if (leaderLevel.Role == "Auto" || leaderLevel.Role == "Approval")
                        {
                            _leader = leaderLevel.UserId;
                            if (string.IsNullOrEmpty(_leader)) return new HandleState("Not found leader");
                            if (leaderLevel.Role == "Auto")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                unlockApprove.LeaderApr = userCurrent;
                                unlockApprove.LeaderAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "Leader";
                            }
                            if (leaderLevel.Role == "Approval")
                            {
                                userLeaderOrManager = leaderLevel.UserId;
                                mailLeaderOrManager = leaderLevel.EmailUser;
                                mailUsersDeputy = leaderLevel.EmailDeputies;
                            }
                        }

                        if (managerLevel.Role == "Auto" || managerLevel.Role == "Approval")
                        {
                            _manager = managerLevel.UserId;
                            if (string.IsNullOrEmpty(_manager)) return new HandleState("Not found manager");
                            if (managerLevel.Role == "Auto" && leaderLevel.Role != "Approval")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED;
                                unlockApprove.ManagerApr = userCurrent;
                                unlockApprove.ManagerAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "Manager";
                            }
                            if (managerLevel.Role == "Approval" && leaderLevel.Role != "Approval")
                            {
                                userLeaderOrManager = managerLevel.UserId;
                                mailLeaderOrManager = managerLevel.EmailUser;
                                mailUsersDeputy = managerLevel.EmailDeputies;
                            }
                        }

                        if (accountantLevel.Role == "Auto" || accountantLevel.Role == "Approval")
                        {
                            _accountant = accountantLevel.UserId;
                            if (string.IsNullOrEmpty(_accountant)) return new HandleState("Not found accountant");
                            if (accountantLevel.Role == "Auto"
                                && managerLevel.Role != "Approval"
                                && leaderLevel.Role != "Approval")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                unlockApprove.AccountantApr = userCurrent;
                                unlockApprove.AccountantAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "Accountant";
                                if (buHeadLevel.Role == "Special")
                                {
                                    unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                    unlockApprove.BuheadApr = unlockApprove.AccountantApr = userCurrent;
                                    unlockApprove.BuheadAprDate = unlockApprove.AccountantAprDate = DateTime.Now;
                                    unlockApprove.LevelApprove = "BOD";
                                }
                            }
                            if (accountantLevel.Role == "Approval"
                                && managerLevel.Role != "Approval"
                                && leaderLevel.Role != "Approval")
                            {
                                userLeaderOrManager = accountantLevel.UserId;
                                mailLeaderOrManager = accountantLevel.EmailUser;
                                mailUsersDeputy = accountantLevel.EmailDeputies;
                            }
                        }

                        if (buHeadLevel.Role == "Auto" || buHeadLevel.Role == "Approval" || buHeadLevel.Role == "Special")
                        {
                            _bhHead = buHeadLevel.UserId;
                            if (string.IsNullOrEmpty(_bhHead)) return new HandleState("Not found BOD");
                            if (buHeadLevel.Role == "Auto"
                                && accountantLevel.Role != "Approval"
                                && managerLevel.Role != "Approval"
                                && leaderLevel.Role != "Approval")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                unlockApprove.BuheadApr = userCurrent;
                                unlockApprove.BuheadAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "BOD";
                            }
                            if ((buHeadLevel.Role == "Approval" || buHeadLevel.Role == "Special")
                                && accountantLevel.Role != "Approval"
                                && managerLevel.Role != "Approval"
                                && leaderLevel.Role != "Approval")
                            {
                                userLeaderOrManager = buHeadLevel.UserId;
                                mailLeaderOrManager = buHeadLevel.EmailUser;
                                mailUsersDeputy = buHeadLevel.EmailDeputies;
                            }
                            if (buHeadLevel.Role == "Special" && (leaderLevel.Role == "None" || leaderLevel.Role == "Auto") && (managerLevel.Role == "None" || managerLevel.Role == "Auto") && (accountantLevel.Role == "None" || accountantLevel.Role == "Auto"))
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                unlockApprove.BuheadApr = userCurrent;
                                unlockApprove.BuheadAprDate = DateTime.Now;
                                unlockApprove.LevelApprove = "BOD";
                                if (leaderLevel.Role != "None")
                                {
                                    unlockApprove.LeaderApr = userCurrent;
                                    unlockApprove.LeaderAprDate = DateTime.Now;
                                }
                                if (managerLevel.Role != "None")
                                {
                                    unlockApprove.ManagerApr = userCurrent;
                                    unlockApprove.ManagerAprDate = DateTime.Now;
                                }
                                if(accountantLevel.Role != "None")
                                {
                                    unlockApprove.AccountantApr = userCurrent;
                                    unlockApprove.AccountantAprDate = DateTime.Now;
                                }
                            }
                        }

                        var sendMailApproved = true;
                        var sendMailSuggest = true;
                        if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                        {
                            //Send Mail Approved
                            sendMailApproved = SendMailApprove(unlockRequest, DateTime.Now);
                        }
                        else
                        {
                            //Send Mail Suggest
                            sendMailSuggest = SendMailSuggest(unlockRequest, userLeaderOrManager, mailLeaderOrManager, mailUsersDeputy);
                        }

                        if (!sendMailSuggest)
                        {
                            return new HandleState("Send mail suggest approval failed");
                        }
                        if (!sendMailApproved)
                        {
                            return new HandleState("Send mail approved approval failed");
                        }
                        else
                        {
                            //Set: Shipment isLocked = False; Advance & Settlement: Status Approval = Denid; Change Service Date: Update SeviceDate = New Service Date
                            var requestJobs = unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job).ToList();
                            UpdatedUnlockRequest(unlockRequest.Id, unlockRequest.UnlockType, requestJobs, unlockRequest.NewServiceDate);
                        }

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
        public bool CheckAllLevelIsAutoOrNone(string type, Guid? officeId)
        {
            var roleLeader = userBaseService.GetRoleByLevel("Leader", type, officeId);
            var roleManager = userBaseService.GetRoleByLevel("Manager", type, officeId);
            var roleAccountant = userBaseService.GetRoleByLevel("Accountant", type, officeId);
            var roleBuHead = userBaseService.GetRoleByLevel("BOD", type, officeId);
            if (
                (roleLeader == "Auto" && roleManager == "Auto" && roleAccountant == "Auto" && roleBuHead == "Auto")
                ||
                (roleLeader == "None" && roleManager == "None" && roleAccountant == "None" && roleBuHead == "None")
               )
            {
                return true;
            }
            return false;
        }

        public InfoLevelApproveResult LeaderLevel(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleLeader = userBaseService.GetRoleByLevel("Leader", type, officeId);
            var userLeader = userBaseService.GetLeaderGroup(companyId, officeId, departmentId, groupId).FirstOrDefault();
            var employeeIdOfLeader = userBaseService.GetEmployeeIdOfUser(userLeader);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userLeader, groupId, departmentId, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userLeader, groupId, departmentId, officeId, companyId);

            result.LevelApprove = "Leader";
            result.Role = roleLeader;
            result.UserId = userLeader;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfLeader)?.Email;
            result.EmailDeputies = emailDeputies;
            
            return result;
        }

        public InfoLevelApproveResult ManagerLevel(string type, int? departmentId, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleManager = userBaseService.GetRoleByLevel("Manager", type, officeId);
            var userManager = userBaseService.GetDeptManager(companyId, officeId, departmentId).FirstOrDefault();
            var employeeIdOfManager = userBaseService.GetEmployeeIdOfUser(userManager);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userManager, null, departmentId, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userManager, null, departmentId, officeId, companyId);

            result.LevelApprove = "Manager";
            result.Role = roleManager;
            result.UserId = userManager;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfManager)?.Email;
            result.EmailDeputies = emailDeputies;
            
            return result;
        }

        public InfoLevelApproveResult AccountantLevel(string type, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleAccountant = userBaseService.GetRoleByLevel("Accountant", type, officeId);
            var userAccountant = userBaseService.GetAccoutantManager(companyId, officeId).FirstOrDefault();
            var employeeIdOfAccountant = userBaseService.GetEmployeeIdOfUser(userAccountant);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userAccountant, null, null, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userAccountant, null, null, officeId, companyId);

            result.LevelApprove = "Accountant";
            result.Role = roleAccountant;
            result.UserId = userAccountant;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfAccountant)?.Email;
            result.EmailDeputies = emailDeputies;
            
            return result;
        }

        public InfoLevelApproveResult BuHeadLevel(string type, Guid? officeId, Guid? companyId)
        {
            var result = new InfoLevelApproveResult();
            var roleBuHead = userBaseService.GetRoleByLevel("BOD", type, officeId);
            var userBuHead = userBaseService.GetBUHead(companyId, officeId).FirstOrDefault();
            var employeeIdOfBuHead = userBaseService.GetEmployeeIdOfUser(userBuHead);

            var userDeputies = userBaseService.GetUsersDeputyByCondition(type, userBuHead, null, null, officeId, companyId);
            var emailDeputies = userBaseService.GetEmailUsersDeputyByCondition(type, userBuHead, null, null, officeId, companyId);

            result.LevelApprove = "BOD";
            result.Role = roleBuHead;
            result.UserId = userBuHead;
            result.UserDeputies = userDeputies;
            result.EmailUser = userBaseService.GetEmployeeByEmployeeId(employeeIdOfBuHead)?.Email;
            result.EmailDeputies = emailDeputies;
            
            return result;
        }
        #endregion -- Info Level Approve --

        #region -- Check Exist, Check Is Manager, Is Approved --
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

        public bool CheckUserIsApproved(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve)
        {
            var isApproved = false;
            if (approve == null) return true;
            var leaderLevel = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

            var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
            var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
            var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
            var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;

            var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);
            var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);

            if (!isLeader && userCurrent.GroupId != SettingConstants.SpecialGroup && userCurrent.UserID == unlockRequest.RequestUser) //Requester
            {
                isApproved = true;
                if (unlockRequest.RequestDate == null)
                {
                    isApproved = false;
                }
            }
            else if (
                        (isLeader && userCurrent.GroupId != SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Leader || userCurrent.UserID == approve.LeaderApr))
                      ||
                        leaderLevel.UserDeputies.Contains(userCurrent.UserID)
                    ) //Leader
            {
                isApproved = true;
                if (string.IsNullOrEmpty(approve.LeaderApr) && leaderLevel.Role != "None")
                {
                    isApproved = false;
                }
            }
            else if (
                        (isManager && !isDeptAccountant && userCurrent.GroupId == SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Manager || userCurrent.UserID == approve.ManagerApr))
                      ||
                        managerLevel.UserDeputies.Contains(currentUser.UserID)
                    ) //Dept Manager
            {
                isApproved = true;
                if (leaderLevel.Role == "Approval" && string.IsNullOrEmpty(approve.LeaderApr))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(approve.ManagerApr) && managerLevel.Role != "None")
                {
                    isApproved = false;
                }

            }
            else if (
                        (isAccountant && isDeptAccountant && userCurrent.GroupId == SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                      ||
                        accountantLevel.UserDeputies.Contains(currentUser.UserID)
                    ) //Accountant Manager
            {
                isApproved = true;
                if (managerLevel.Role == "Approval" && string.IsNullOrEmpty(approve.ManagerApr))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(approve.AccountantApr) && accountantLevel.Role != "None")
                {
                    isApproved = false;
                }
            }
            else if (
                        (userCurrent.GroupId == SettingConstants.SpecialGroup && isBuHead && isBod && (userCurrent.UserID == approve.Buhead || userCurrent.UserID == approve.BuheadApr))
                      ||
                        buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                    ) //BUHead
            {
                isApproved = true;
                if (buHeadLevel.Role != "Special")
                {
                    if (accountantLevel.Role == "Approval" && string.IsNullOrEmpty(approve.AccountantApr))
                    {
                        return true;
                    }
                }
                if (
                    (string.IsNullOrEmpty(approve.BuheadApr) && buHeadLevel.Role != "None")
                    ||
                    (buHeadLevel.Role == "Special" && !string.IsNullOrEmpty(unlockRequest.RequestUser))
                   )
                {
                    isApproved = false;
                }
            }
            else
            {
                //Đây là trường hợp các User không thuộc Approve
                isApproved = true;
            }
            return isApproved;
        }

        public bool CheckUserIsManager(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve)
        {
            var isManagerOrLeader = false;

            var leaderLevel = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

            var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
            var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
            var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
            var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;

            if (approve == null)
            {
                if ((isLeader && userCurrent.GroupId != SettingConstants.SpecialGroup) || leaderLevel.UserDeputies.Contains(userCurrent.UserID)) //Leader
                {
                    isManagerOrLeader = true;
                }
                else if ((isManager && userCurrent.GroupId == SettingConstants.SpecialGroup) || managerLevel.UserDeputies.Contains(currentUser.UserID)) //Dept Manager
                {
                    isManagerOrLeader = true;
                }
                else if ((isAccountant && userCurrent.GroupId == SettingConstants.SpecialGroup) || accountantLevel.UserDeputies.Contains(currentUser.UserID)) //Accountant Manager
                {
                    isManagerOrLeader = true;
                }
                else if ((isBuHead && userCurrent.GroupId == SettingConstants.SpecialGroup) || buHeadLevel.UserDeputies.Contains(userCurrent.UserID)) //BUHead
                {
                    isManagerOrLeader = true;
                }
            }
            else
            {
                if (
                     (isLeader && userCurrent.GroupId != SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Leader || userCurrent.UserID == approve.LeaderApr))
                     ||
                     leaderLevel.UserDeputies.Contains(userCurrent.UserID)
                   ) //Leader
                {
                    isManagerOrLeader = true;
                }
                else if (
                            (isManager && userCurrent.GroupId == SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Manager || userCurrent.UserID == approve.ManagerApr))
                          ||
                            managerLevel.UserDeputies.Contains(currentUser.UserID)
                        ) //Dept Manager
                {
                    isManagerOrLeader = true;
                }
                else if (
                            (userCurrent.GroupId == SettingConstants.SpecialGroup && isAccountant && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                          ||
                            accountantLevel.UserDeputies.Contains(currentUser.UserID)
                        ) //Accountant Manager
                {
                    isManagerOrLeader = true;
                }
                else if (
                            (userCurrent.GroupId == SettingConstants.SpecialGroup && isBuHead && (userCurrent.UserID == approve.Buhead || userCurrent.UserID == approve.BuheadApr))
                          ||
                            buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                        ) //BUHead
                {
                    isManagerOrLeader = true;
                }
            }
            return isManagerOrLeader;
        }

        public bool CheckIsShowBtnDeny(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve)
        {
            if (approve == null) return false;

            var leaderLevel = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
            var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

            var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
            var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
            var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
            var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;

            var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);
            var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);

            var isShowBtnDeny = false;

            if (
                 ((isLeader && userCurrent.GroupId != SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Leader || userCurrent.UserID == approve.LeaderApr))
                 ||
                 leaderLevel.UserDeputies.Contains(userCurrent.UserID))
               ) //Leader
            {
                if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_REQUESTAPPROVAL || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_LEADERAPPROVED)
                {
                    isShowBtnDeny = true;
                }
                else
                {
                    isShowBtnDeny = false;
                }
            }
            else if (
                        ((isManager && !isDeptAccountant && userCurrent.GroupId == SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Manager || userCurrent.UserID == approve.ManagerApr))
                      ||
                        managerLevel.UserDeputies.Contains(currentUser.UserID))

                    ) //Dept Manager
            {
                if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_LEADERAPPROVED || (leaderLevel.Role == "None" && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DONE))
                {
                    isShowBtnDeny = true;
                }
                else
                {
                    isShowBtnDeny = false;
                }
            }
            else if (
                       ((isAccountant && isDeptAccountant && userCurrent.GroupId == SettingConstants.SpecialGroup && (userCurrent.UserID == approve.Accountant || userCurrent.UserID == approve.AccountantApr))
                      ||
                        accountantLevel.UserDeputies.Contains(currentUser.UserID))
                    ) //Accountant Manager
            {
                if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED || (managerLevel.Role == "None" && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DONE))
                {
                    isShowBtnDeny = true;
                }
                else
                {
                    isShowBtnDeny = false;
                }
            }
            else if (buHeadLevel.Role == "Special" && isBod
                &&
                (
                  (isBuHead && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent.UserID == buHeadLevel.UserId)
                  ||
                  buHeadLevel.UserDeputies.Contains(userCurrent.UserID)
                )
                && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_NEW && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DENIED && unlockRequest.StatusApproval != SettingConstants.STATUS_APPROVAL_DONE
               )
            {
                isShowBtnDeny = true;
            }
            else
            {
                isShowBtnDeny = false;
            }
            return isShowBtnDeny;
        }
        #endregion -- Check Exist, Check Is Manager, Is Approved --

        #region -- APPROVE, DENY, CANCEL REQUEST --
        public HandleState UpdateApproval(Guid id)
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

                    var isAllLevelAutoOrNone = CheckAllLevelIsAutoOrNone(unlockRequest.UnlockType, unlockRequest.OfficeId);
                    if (isAllLevelAutoOrNone)
                    {
                        //Cập nhật Status Approval là Done cho Unlock Request
                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                    }

                    var leaderLevel = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

                    var userApproveNext = string.Empty;
                    var mailUserApproveNext = string.Empty;
                    List<string> mailUsersDeputy = new List<string>();

                    var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);

                    var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
                    if (leaderLevel.Role == "Approval"
                         &&
                         (
                           (isLeader && currentUser.GroupId != SettingConstants.SpecialGroup && userCurrent == leaderLevel.UserId)
                           ||
                           leaderLevel.UserDeputies.Contains(userCurrent)
                          )
                        )
                    {
                        if (string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            if (!string.IsNullOrEmpty(unlockRequest.RequestUser))
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_LEADERAPPROVED;
                                approve.LeaderApr = userCurrent;
                                approve.LeaderAprDate = DateTime.Now;
                                approve.LevelApprove = "Leader";
                                userApproveNext = managerLevel.UserId;
                                mailUserApproveNext = managerLevel.EmailUser;
                                mailUsersDeputy = managerLevel.EmailDeputies;
                                if (managerLevel.Role == "Auto" || managerLevel.Role == "None")
                                {
                                    if (managerLevel.Role == "Auto")
                                    {
                                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED;
                                        approve.ManagerApr = managerLevel.UserId;
                                        approve.ManagerAprDate = DateTime.Now;
                                        approve.LevelApprove = "Manager";
                                        userApproveNext = accountantLevel.UserId;
                                        mailUserApproveNext = accountantLevel.EmailUser;
                                        mailUsersDeputy = accountantLevel.EmailDeputies;
                                    }
                                    if (accountantLevel.Role == "Auto" || accountantLevel.Role == "None")
                                    {
                                        if (accountantLevel.Role == "Auto")
                                        {
                                            unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                            approve.AccountantApr = accountantLevel.UserId;
                                            approve.AccountantAprDate = DateTime.Now;
                                            approve.LevelApprove = "Accountant";
                                            userApproveNext = buHeadLevel.UserId;
                                            mailUserApproveNext = buHeadLevel.EmailUser;
                                            mailUsersDeputy = buHeadLevel.EmailDeputies;
                                        }
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
                            return new HandleState("Leader approved");
                        }
                    }

                    var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
                    if (managerLevel.Role == "Approval" && !isDeptAccountant
                        &&
                        (
                           (isManager && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent == managerLevel.UserId)
                           ||
                           managerLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (leaderLevel.Role == "Approval" && string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            return new HandleState("Leader not approve");
                        }
                        if (string.IsNullOrEmpty(approve.ManagerApr))
                        {
                            if (!string.IsNullOrEmpty(approve.LeaderApr) || leaderLevel.Role == "None" || leaderLevel.Role == "Auto")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED;
                                approve.ManagerApr = userCurrent;
                                approve.ManagerAprDate = DateTime.Now;
                                approve.LevelApprove = "Manager";
                                userApproveNext = accountantLevel.UserId;
                                mailUserApproveNext = accountantLevel.EmailUser;
                                mailUsersDeputy = accountantLevel.EmailDeputies;
                                if (accountantLevel.Role == "Auto" || accountantLevel.Role == "None")
                                {
                                    if (accountantLevel.Role == "Auto")
                                    {
                                        unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                        approve.AccountantApr = accountantLevel.UserId;
                                        approve.AccountantAprDate = DateTime.Now;
                                        approve.LevelApprove = "Accountant";
                                        userApproveNext = buHeadLevel.UserId;
                                        mailUserApproveNext = buHeadLevel.EmailUser;
                                        mailUsersDeputy = buHeadLevel.EmailDeputies;
                                    }
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

                    var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    if (accountantLevel.Role == "Approval" && isDeptAccountant
                        &&
                        (
                           (isAccountant && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent == accountantLevel.UserId)
                           ||
                           accountantLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (managerLevel.Role == "Approval" && string.IsNullOrEmpty(approve.ManagerApr))
                        {
                            return new HandleState("The manager has not approved it yet");
                        }
                        if (string.IsNullOrEmpty(approve.AccountantApr))
                        {
                            if (!string.IsNullOrEmpty(approve.ManagerApr) || managerLevel.Role == "None" || managerLevel.Role == "Auto")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED;
                                approve.AccountantApr = userCurrent;
                                approve.AccountantAprDate = DateTime.Now;
                                approve.LevelApprove = "Accountant";
                                userApproveNext = buHeadLevel.UserId;
                                mailUserApproveNext = buHeadLevel.EmailUser;
                                mailUsersDeputy = buHeadLevel.EmailDeputies;
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

                    var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                    if ((buHeadLevel.Role == "Approval" || buHeadLevel.Role == "Special") && isBod
                        &&
                        (
                          (isBuHead && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent == buHeadLevel.UserId)
                          ||
                          buHeadLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (buHeadLevel.Role != "Special")
                        {
                            if (accountantLevel.Role == "Approval" && string.IsNullOrEmpty(approve.AccountantApr))
                            {
                                return new HandleState("The accountant has not approved it yet");
                            }
                        }
                        if (string.IsNullOrEmpty(approve.BuheadApr))
                        {
                            if (!string.IsNullOrEmpty(approve.AccountantApr) || accountantLevel.Role == "None" || accountantLevel.Role == "Auto" || buHeadLevel.Role == "Special")
                            {
                                unlockRequest.StatusApproval = SettingConstants.STATUS_APPROVAL_DONE;
                                approve.BuheadApr = userCurrent;
                                approve.BuheadAprDate = DateTime.Now;
                                approve.LevelApprove = "BOD";
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

                    var sendMailApproved = true;
                    var sendMailSuggest = true;

                    if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                    {
                        //Send Mail Approved
                        sendMailApproved = SendMailApprove(unlockRequest, DateTime.Now);
                    }
                    else
                    {
                        //Send Mail Suggest
                        sendMailSuggest = SendMailSuggest(unlockRequest, userApproveNext, mailUserApproveNext, mailUsersDeputy);
                    }

                    if (!sendMailSuggest)
                    {
                        return new HandleState("Send mail suggest Approval failed");
                    }
                    if (!sendMailApproved)
                    {
                        return new HandleState("Send mail Approved Approval failed");
                    }
                    else
                    {
                        //Set: Shipment isLocked = False; Advance & Settlement: Status Approval = Denid; Change Service Date: Update SeviceDate = New Service Date
                        var requestJobs = unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job).ToList();
                        UpdatedUnlockRequest(unlockRequest.Id, unlockRequest.UnlockType, requestJobs, unlockRequest.NewServiceDate);
                    }

                    unlockRequest.UserModified = approve.UserModified = userCurrent;
                    unlockRequest.DatetimeModified = approve.DatetimeModified = DateTime.Now;

                    var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
                    var hsUpdateApprove = DataContext.Update(approve, x => x.Id == approve.Id);

                    unlockRequestRepo.SubmitChanges();
                    DataContext.SubmitChanges();
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

                    var leaderLevel = LeaderLevel(unlockRequest.UnlockType, unlockRequest.GroupId, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var managerLevel = ManagerLevel(unlockRequest.UnlockType, unlockRequest.DepartmentId, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var accountantLevel = AccountantLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);
                    var buHeadLevel = BuHeadLevel(unlockRequest.UnlockType, unlockRequest.OfficeId, unlockRequest.CompanyId);

                    var isApprover = false;
                    var isDeptAccountant = userBaseService.CheckIsAccountantDept(currentUser.DepartmentId);

                    var isLeader = userBaseService.GetLeaderGroup(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId, currentUser.GroupId).FirstOrDefault() == currentUser.UserID;
                    if (leaderLevel.Role == "Approval"
                        &&
                        (
                            (isLeader && currentUser.GroupId != SettingConstants.SpecialGroup && userCurrent == leaderLevel.UserId)
                            ||
                            leaderLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_MANAGERAPPROVED
                            || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                            || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Unlock request has been approved");
                        }

                        if (string.IsNullOrEmpty(unlockRequest.RequestUser))
                        {
                            return new HandleState("The requester has not send the request yet");
                        }
                        approve.LeaderApr = userCurrent;
                        approve.LeaderAprDate = DateTime.Now;
                        approve.LevelApprove = "Leader";

                        isApprover = true;
                    }

                    var isManager = userBaseService.GetDeptManager(currentUser.CompanyID, currentUser.OfficeID, currentUser.DepartmentId).FirstOrDefault() == currentUser.UserID;
                    if (managerLevel.Role == "Approval" && !isDeptAccountant
                        &&
                        (
                            (isManager && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent == managerLevel.UserId)
                            ||
                            managerLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_ACCOUNTANTAPPRVOVED
                            || unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Unlock request has been approved");
                        }

                        if (leaderLevel.Role == "Approval" && string.IsNullOrEmpty(approve.LeaderApr))
                        {
                            return new HandleState("Leader not approve");
                        }
                        approve.ManagerApr = userCurrent;
                        approve.ManagerAprDate = DateTime.Now;
                        approve.LevelApprove = "Manager";

                        isApprover = true;
                    }

                    var isAccountant = userBaseService.GetAccoutantManager(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    if (accountantLevel.Role == "Approval" && isDeptAccountant
                        &&
                        (
                            (isAccountant && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent == accountantLevel.UserId)
                            ||
                            accountantLevel.UserDeputies.Contains(userCurrent)
                         )
                        )
                    {
                        if (unlockRequest.StatusApproval == SettingConstants.STATUS_APPROVAL_DONE)
                        {
                            return new HandleState("Not allow deny. Unlock request has been approved");
                        }

                        if (managerLevel.Role == "Approval" && string.IsNullOrEmpty(approve.ManagerApr))
                        {
                            return new HandleState("The manager has not approved it yet");
                        }
                        approve.AccountantApr = userCurrent;
                        approve.AccountantAprDate = DateTime.Now;
                        approve.LevelApprove = "Accountant";

                        isApprover = true;
                    }

                    var isBuHead = userBaseService.GetBUHead(currentUser.CompanyID, currentUser.OfficeID).FirstOrDefault() == currentUser.UserID;
                    var isBod = userBaseService.CheckIsBOD(currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                    if (buHeadLevel.Role == "Approval" && isBod
                        &&
                        (
                          (isBuHead && currentUser.GroupId == SettingConstants.SpecialGroup && userCurrent == buHeadLevel.UserId)
                          ||
                          buHeadLevel.UserDeputies.Contains(userCurrent)
                        )
                       )
                    {
                        if (accountantLevel.Role == "Approval" && string.IsNullOrEmpty(approve.AccountantApr))
                        {
                            return new HandleState("The accountant has not approved it yet");
                        }
                        approve.BuheadApr = userCurrent;
                        approve.BuheadAprDate = DateTime.Now;
                        approve.LevelApprove = "BOD";

                        isApprover = true;
                    }

                    if (!isApprover)
                    {
                        return new HandleState("Not allow deny. You are not in the approval process.");
                    }

                    var sendMailDeny = SendMailDeny(unlockRequest, comment, DateTime.Now);
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
                    DataContext.SubmitChanges();
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
                        unlockRequest.RequestUser = null;
                        unlockRequest.RequestDate = null;
                        unlockRequest.UserModified = userCurrent;
                        unlockRequest.DatetimeModified = DateTime.Now;
                        var hsUpdateUnlockRequest = unlockRequestRepo.Update(unlockRequest, x => x.Id == unlockRequest.Id, false);
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
        #endregion -- APPROVE, DENY, CANCEL REQUEST --

        #region -- TEMPLATE & SEND EMAIL --
        private string GetTableServiceDate(SetUnlockRequest unlockRequest)
        {
            var requestJobs = unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id);
            var opss = opsTransactionRepo.Get();
            var csTrans = csTransactionRepo.Get();
            var dataOps = (from job in requestJobs
                           join ops in opss on job.Job equals ops.JobNo
                           select new OpsTransaction
                           {
                               JobNo = job.Job,
                               ServiceDate = ops.ServiceDate
                           }).ToList();
            var dataDoc = (from job in requestJobs
                           join cst in csTrans on job.Job equals cst.JobNo
                           select new CsTransaction
                           {
                               JobNo = job.Job,
                               ServiceDate = cst.ServiceDate
                           }).ToList();

            var dataJob = string.Empty;
            if (dataOps != null && dataOps.Count > 0)
            {
                foreach (var item in dataOps)
                {
                    string _serviceDate = item.ServiceDate != null ? item.ServiceDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                    string _newServiceDate = unlockRequest.NewServiceDate != null ? unlockRequest.NewServiceDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                    dataJob += "<tr><td style='border: 1px solid #dddddd; width: 40%;'>" + item.JobNo + "</td><td style='border: 1px solid #dddddd; width: 30%;'>" + _serviceDate + "</td><td style='border: 1px solid #dddddd; width: 30%;'>" + _newServiceDate + "</td></tr>";
                }
            }
            if (dataDoc != null && dataDoc.Count > 0)
            {
                foreach (var item in dataDoc)
                {
                    string _serviceDate = item.ServiceDate != null ? item.ServiceDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                    string _newServiceDate = unlockRequest.NewServiceDate != null ? unlockRequest.NewServiceDate.Value.ToString("dd/MM/yyyy") : string.Empty;
                    dataJob += "<tr><td style='border: 1px solid #dddddd; width: 40%;'>" + item.JobNo + "</td><td style='border: 1px solid #dddddd; width: 30%;'>" + _serviceDate + "</td><td style='border: 1px solid #dddddd; width: 30%;'>" + _newServiceDate + "</td></tr>";
                }
            }

            var result = "<table style='width: 100%; border: 1px solid #dddddd;border-collapse: collapse;'><tr><td style='border: 1px solid #dddddd; font-weight: bold;'>JobID</td><td style='border: 1px solid #dddddd; font-weight: bold;'>Service Date</td><td style='border: 1px solid #dddddd; font-weight: bold;'>New Service Date</td></tr>" + dataJob + "</table>";
            return result;
        }

        private bool SendMailSuggestUnlockShipment(SetUnlockRequest unlockRequest, string userReciver, string mailUserReciver, List<string> mailUsersDeputy)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //Lấy ra tên của user Approval
            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //List String Shipment
            var jobIds = string.Join("; ", unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job));

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock Shipment";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [UserName],</b></i></p><p>You have new Unlock Approval Request from <b>[RequesterName]</b> as below info: </p><p><i>Anh/ Chị có một yêu cầu duyệt mở khóa từ <b>[RequesterName]</b> với thông tin như sau:</i></p><p><i><b>List Shipment:</b></i> [JobIds]</p><p>[GeneralReason]</p><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[UserName]", userReciverName);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[GeneralReason]", unlockRequest.GeneralReason);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                mailUserReciver
            };
            List<string> attachments = null;

            //CC cho User Requester để biết được quá trình Approve đã đến bước nào. Và các User được ủy quyền
            List<string> emailCCs = new List<string>();
            emailCCs = mailUsersDeputy;
            emailCCs.Add(emailRequester);

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailSuggestUnlockAdvanceOrSettlement(SetUnlockRequest unlockRequest, string userReciver, string mailUserReciver, List<string> mailUsersDeputy)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //Lấy ra tên của user Approval
            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //List String Advance/ Settlement
            var jobIds = string.Join("\r\n", unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job));

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock " + unlockRequest.UnlockType;
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [UserName],</b></i></p><p>You have new Unlock Approval Request from <b>[RequesterName]</b> as below info: </p><p><i>Anh/ Chị có một yêu cầu duyệt mở khóa từ <b>[RequesterName]</b> với thông tin như sau:</i></p><p><i><b>List [Type]:</b></i> [JobIds]</p><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[UserName]", userReciverName);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[Type]", unlockRequest.UnlockType);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                mailUserReciver
            };
            List<string> attachments = null;

            //CC cho User Requester để biết được quá trình Approve đã đến bước nào. Và các User được ủy quyền
            List<string> emailCCs = new List<string>();
            emailCCs = mailUsersDeputy;
            emailCCs.Add(emailRequester);

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailSuggestChangeServiceDate(SetUnlockRequest unlockRequest, string userReciver, string mailUserReciver, List<string> mailUsersDeputy)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //Lấy ra tên của user Approval
            var userReciverId = userBaseService.GetEmployeeIdOfUser(userReciver);
            var userReciverName = userBaseService.GetEmployeeByEmployeeId(userReciverId)?.EmployeeNameEn;

            //List Job ID, Service Date
            var jobIds = GetTableServiceDate(unlockRequest);

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock Change Service Date";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [UserName],</b></i></p><p>You have new Unlock Approval Request from <b>[RequesterName]</b> as below info: </p><p><i>Anh/ Chị có một yêu cầu duyệt mở khóa từ <b>[RequesterName]</b> với thông tin như sau:</i></p><p>[JobIds]</p><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[UserName]", userReciverName);
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                mailUserReciver
            };
            List<string> attachments = null;

            //CC cho User Requester để biết được quá trình Approve đã đến bước nào. Và các User được ủy quyền
            List<string> emailCCs = new List<string>();
            emailCCs = mailUsersDeputy;
            emailCCs.Add(emailRequester);

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailSuggest(SetUnlockRequest unlockRequest, string userReciver, string mailUserReciver, List<string> mailUsersDeputy)
        {
            var sendMailSuggest = false;
            switch (unlockRequest.UnlockType)
            {
                case "Shipment":
                    sendMailSuggest = SendMailSuggestUnlockShipment(unlockRequest, userReciver, mailUserReciver, mailUsersDeputy);
                    break;
                case "Advance":
                    sendMailSuggest = SendMailSuggestUnlockAdvanceOrSettlement(unlockRequest, userReciver, mailUserReciver, mailUsersDeputy);
                    break;
                case "Settlement":
                    sendMailSuggest = SendMailSuggestUnlockAdvanceOrSettlement(unlockRequest, userReciver, mailUserReciver, mailUsersDeputy);
                    break;
                case "Change Service Date":
                    sendMailSuggest = SendMailSuggestChangeServiceDate(unlockRequest, userReciver, mailUserReciver, mailUsersDeputy);
                    break;
            }
            return sendMailSuggest;
        }

        private bool SendMailApproveUnlockShipment(SetUnlockRequest unlockRequest, DateTime approvedDate)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //List String Shipment
            var jobIds = string.Join("; ", unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job));

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock Shipment is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have new Unlock Approval Request is approved at <b>[ApprovedDate]</b> as below info: </p><p><i>Anh/ Chị có một đề nghị mở khóa đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với các thông tin như sau:</i></p><p><i><b>List Shipment:</b></i> [JobIds]</p><p>[GeneralReason]</p><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[GeneralReason]", unlockRequest.GeneralReason);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;

            //Không cần CC
            List<string> emailCCs = new List<string>();

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailApproveUnlockAdvanceOrSettlement(SetUnlockRequest unlockRequest, DateTime approvedDate)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //List String Shipment
            var jobIds = string.Join("; ", unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job));

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock " + unlockRequest.UnlockType + " is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have new Unlock Approval Request is approved at <b>[ApprovedDate]</b> as below info: </p><p><i>Anh/ Chị có một đề nghị mở khóa đã được phê duyệt vào lúc <b>[ApprovedDate]</b> với các thông tin như sau:</i></p><p><i><b>List [Type]:</b></i> [JobIds]</p><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[Type]", unlockRequest.UnlockType);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            //Không cần CC
            List<string> emailCCs = new List<string>();

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailApproveChangeServiceDate(SetUnlockRequest unlockRequest, DateTime approvedDate)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //List Job ID, Service Date
            var jobIds = GetTableServiceDate(unlockRequest);

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock Change Service Date is approved";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have new Unlock Approval Request from <b>[RequesterName]</b> as below info: </p><p><i>Anh/ Chị có một yêu cầu duyệt mở khóa từ <b>[RequesterName]</b> với thông tin như sau:</i></p><p>[JobIds]</p><p>You click here to check more detail and approve: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để biết thêm thông tin chi tiết và phê duyệt: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[ApprovedDate]", approvedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;
            //Không cần CC
            List<string> emailCCs = new List<string>();

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailApprove(SetUnlockRequest unlockRequest, DateTime approvedDate)
        {
            var sendMailApproved = false;
            switch (unlockRequest.UnlockType)
            {
                case "Shipment":
                    sendMailApproved = SendMailApproveUnlockShipment(unlockRequest, DateTime.Now);
                    break;
                case "Advance":
                    sendMailApproved = SendMailApproveUnlockAdvanceOrSettlement(unlockRequest, DateTime.Now);
                    break;
                case "Settlement":
                    sendMailApproved = SendMailApproveUnlockAdvanceOrSettlement(unlockRequest, DateTime.Now);
                    break;
                case "Change Service Date":
                    sendMailApproved = SendMailApproveChangeServiceDate(unlockRequest, DateTime.Now);
                    break;
            }
            return sendMailApproved;
        }

        private bool SendMailDenyUnlockShipment(SetUnlockRequest unlockRequest, string comment, DateTime deniedDate)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //List String Shipment
            var jobIds = string.Join("; ", unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job));

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock Shipment is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have new Unlock Approval Request is denied at <b>[DeniedDate]</b> as below info: </p><p><i>Anh/ Chị có một đề nghị bị từ chối vào lúc <b>[DeniedDate]</b> với thông tin như sau: </i></p><ul><li><p><i><b>List Shipment:</b></i> [JobIds]</p><p>[GeneralReason]</p></li><li><p><i>Comment/ Lý do từ chối: </i>[Comment]</p></li></ul><p>You click here to recheck detail: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", deniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[GeneralReason]", unlockRequest.GeneralReason);
            body = body.Replace("[Comment]", comment);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;

            //Không cần CC
            List<string> emailCCs = new List<string>();

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailDenyUnlockAdvanceOrSettlement(SetUnlockRequest unlockRequest, string comment, DateTime deniedDate)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //List String Shipment
            var jobIds = string.Join("; ", unlockRequestJobRepo.Get(x => x.UnlockRequestId == unlockRequest.Id).Select(s => s.Job));

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock " + unlockRequest.UnlockType + " is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have new Unlock Approval Request is denied at <b>[DeniedDate]</b> as below info: </p><p><i>Anh/ Chị có một đề nghị bị từ chối vào lúc <b>[DeniedDate]</b> với thông tin như sau:</i></p><ul><li><p><i><b>List [Type]:</b></i> [JobIds]</p></li><li><p><i>Comment/ Lý do từ chối: </i>[Comment]</p></li></ul><p>You click here to recheck detail: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", deniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[Type]", unlockRequest.UnlockType);
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[Comment]", comment);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;

            //Không cần CC
            List<string> emailCCs = new List<string>();

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailDenyChangeServiceDate(SetUnlockRequest unlockRequest, string comment, DateTime deniedDate)
        {
            //Lấy ra tên & email của user Requester
            var requesterId = userBaseService.GetEmployeeIdOfUser(unlockRequest.Requester);
            var requesterName = userBaseService.GetEmployeeByEmployeeId(requesterId)?.EmployeeNameEn;
            var emailRequester = userBaseService.GetEmployeeByEmployeeId(requesterId)?.Email;

            //List Job ID, Service Date
            var jobIds = GetTableServiceDate(unlockRequest);

            string subject = "eFMS - Unlock Approval Request from [RequesterName] - Unlock Change Service Date is denied";
            subject = subject.Replace("[RequesterName]", requesterName);
            string body = string.Format(@"<div style='font-family: Calibri; font-size: 12pt'><p><i><b>Dear Mr/Mrs [RequesterName],</b></i></p><p>You have new Unlock Approval Request is denied at <b>[DeniedDate]</b> as below info: </p><p><i>Anh/ Chị có một đề nghị bị từ chối vào lúc <b>[DeniedDate]</b> với thông tin như sau:</i></p><p>[JobIds]</p><ul><li><p></p><i>Comment/ Lý do từ chối: </i>[Comment]</p></li></ul><p>You click here to recheck detail: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Detail Unlock Request</a></span></p><p><i>Anh/ Chị chọn vào đây để kiểm tra lại thông tin chi tiết: <span><a href='[Url]/[lang]/[UrlFunc]/[UnlockRequestId]' target='_blank'>Chi tiết phiếu đề nghị mở khóa</a></span></i></p><p>Thanks and Regards,<p><p><b>eFMS System,</b></p><p><img src='[logoEFMS]'/></p></div>");
            body = body.Replace("[RequesterName]", requesterName);
            body = body.Replace("[DeniedDate]", deniedDate.ToString("HH:mm - dd/MM/yyyy"));
            body = body.Replace("[JobIds]", jobIds);
            body = body.Replace("[Comment]", comment);
            body = body.Replace("[Url]", webUrl.Value.Url.ToString());
            body = body.Replace("[lang]", "en");
            body = body.Replace("[UrlFunc]", "#/home/tool/unlock-request");
            body = body.Replace("[UnlockRequestId]", unlockRequest.Id.ToString());
            body = body.Replace("[logoEFMS]", apiUrl.Value.Url.ToString() + "/ReportPreview/Images/logo-eFMS.png");

            List<string> toEmails = new List<string> {
                emailRequester
            };
            List<string> attachments = null;

            //Không cần CC
            List<string> emailCCs = new List<string>();

            var sendMailResult = SendMail.Send(subject, body, toEmails, attachments, emailCCs);
            return sendMailResult;
        }

        private bool SendMailDeny(SetUnlockRequest unlockRequest, string comment, DateTime deniedDate)
        {
            var sendMailDeny = false;
            switch (unlockRequest.UnlockType)
            {
                case "Shipment":
                    sendMailDeny = SendMailDenyUnlockShipment(unlockRequest, comment, DateTime.Now);
                    break;
                case "Advance":
                    sendMailDeny = SendMailDenyUnlockAdvanceOrSettlement(unlockRequest, comment, DateTime.Now);
                    break;
                case "Settlement":
                    sendMailDeny = SendMailDenyUnlockAdvanceOrSettlement(unlockRequest, comment, DateTime.Now);
                    break;
                case "Change Service Date":
                    sendMailDeny = SendMailDenyChangeServiceDate(unlockRequest, comment, DateTime.Now);
                    break;
            }
            return sendMailDeny;
        }
        #endregion -- TEMPLATE & SEND EMAIL --

        #region -- UPDATED UNLOCK SHIPMENT, ADVANCE, SETTLEMENT, CHANGE SERVICE DATE --
        public void UpdatedUnlockRequest(Guid id, string unlockType, List<string> jobs, DateTime? newServiceDate)
        {
            switch (unlockType)
            {
                case "Shipment":
                case "Change Service Date":
                    UpdatedUnlockShipmentOrChangeServiceDate(id, unlockType, jobs, newServiceDate);
                    break;
                case "Advance":
                    UpdatedUnlockAdvance(id, jobs);
                    break;
                case "Settlement":
                    UpdatedUnlockSettlement(id, jobs);
                    break;
            }
        } 

        private void UpdatedUnlockShipmentOrChangeServiceDate(Guid id, string type, List<string> jobs, DateTime? newServiceDate)
        {            
            foreach(var job in jobs)
            {
                var ops = opsTransactionRepo.Get(x => x.JobNo == job).FirstOrDefault();
                if (ops != null)
                {
                    if (type == "Shipment")
                    {
                        ops.IsLocked = false;
                    }
                    if (type == "Change Service Date")
                    {
                        ops.ServiceDate = newServiceDate;
                    }
                    ops.UserModified = currentUser.UserID;
                    ops.DatetimeModified = DateTime.Now;
                    opsTransactionRepo.Update(ops, x => x.Id == ops.Id, false);
                }
                else
                {
                    var doc = csTransactionRepo.Get(x => x.JobNo == job).FirstOrDefault();
                    doc.IsLocked = false;
                    doc.UserModified = currentUser.UserID;
                    doc.DatetimeModified = DateTime.Now;
                    csTransactionRepo.Update(doc, x => x.Id == doc.Id, false);
                }
            }
        }

        private void UpdatedUnlockAdvance(Guid id, List<string> jobs)
        {
            foreach(var job in jobs)
            {
                var adv = advancePaymentRepo.Get(x => x.AdvanceNo == job).FirstOrDefault();
                adv.StatusApproval = SettingConstants.STATUS_APPROVAL_DENIED;
                adv.UserModified = currentUser.UserID;
                adv.DatetimeModified = DateTime.Now;
                advancePaymentRepo.Update(adv, x => x.Id == adv.Id, false);

                var aprAdv = approveAdvanceRepo.Get(x => x.AdvanceNo == adv.AdvanceNo && x.IsDeputy == false).FirstOrDefault();
                aprAdv.IsDeputy = true;
                aprAdv.Comment = "UNLOCK REQUEST ADVANCE";
                approveAdvanceRepo.Update(aprAdv, x => x.Id == aprAdv.Id, false);
            }
        }

        private void UpdatedUnlockSettlement(Guid id, List<string> jobs)
        {
            foreach (var job in jobs)
            {
                var settle = settlementPaymentRepo.Get(x => x.SettlementNo == job).FirstOrDefault();
                settle.StatusApproval = SettingConstants.STATUS_APPROVAL_DENIED;
                settle.UserModified = currentUser.UserID;
                settle.DatetimeModified = DateTime.Now;
                settlementPaymentRepo.Update(settle, x => x.Id == settle.Id, false);

                var aprSettle = approveSettlementRepo.Get(x => x.SettlementNo == settle.SettlementNo && x.IsDeputy == false).FirstOrDefault();
                aprSettle.IsDeputy = true;
                aprSettle.Comment = "UNLOCK REQUEST SETTLEMENT";
                approveSettlementRepo.Update(aprSettle, x => x.Id == aprSettle.Id, false);
            }
        }
        
        #endregion -- UPDATED UNLOCK SHIPMENT, ADVANCE, SETTLEMENT, CHANGE SERVICE DATE --
    }
}
