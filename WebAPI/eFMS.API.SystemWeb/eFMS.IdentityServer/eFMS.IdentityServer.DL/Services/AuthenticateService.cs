using AutoMapper;
using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Linq;
using eFMS.IdentityServer.DL.IService;
using eFMS.API.System.DL.ViewModels;
using eFMS.IdentityServer.DL.Models;
using System;
using eFMS.IdentityServer.DL.Helpers;
using eFMS.IdentityServer.DL.Infrastructure;
using Microsoft.Extensions.Options;
using System.DirectoryServices;
using System.Net;
using ITL.NetCore.Common;
using System.Net.Sockets;

namespace eFMS.IdentityServer.DL.Services
{
    public class AuthenticateService : RepositoryBase<SysUser, UserModel>, IAuthenUserService
    {
        protected ISysUserLogService userLogService;
        readonly LDAPConfig ldap;
        IContextBase<SysEmployee> employeeRepository;
        public readonly IContextBase<SysUserLevel> userLevelRepository;



        public AuthenticateService(
            IContextBase<SysUser> repository, IMapper mapper,
            ISysUserLogService logService,
            IOptions<LDAPConfig> ldapConfig,
            IContextBase<SysUserLevel> userLevelRepo,
            IContextBase<SysEmployee> employeeRepo) : base(repository, mapper)
        {
            userLogService = logService;
            ldap = ldapConfig.Value;
            employeeRepository = employeeRepo;
            userLevelRepository = userLevelRepo;

        }

        public UserViewModel GetUserById(string id)
        {
            var user = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (user == null) return null;
            var employee = employeeRepository.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
            var result = new UserViewModel();

            result.Id = user.Id;
            result.Username = user.Username;
            result.EmployeeId = user.EmployeeId;
            result.WorkPlaceId = user.WorkPlaceId;
            result.RefuseEmail = user.RefuseEmail;
            result.LdapObjectGuid = user.LdapObjectGuid;
            result.DepartmentId = employee?.DepartmentId;
            result.EmployeeNameVn = employee?.EmployeeNameVn;
            result.EmployeeNameEn = employee?.EmployeeNameEn;
            result.Position = employee?.Position;
            result.Birthday = employee?.Birthday;
            result.ExtNo = employee?.ExtNo;
            result.Tel = employee?.Tel;
            result.HomePhone = employee?.HomePhone;
            result.HomeAddress = employee?.HomeAddress;
            result.Email = employee?.Email;
            result.Photo = employee?.Photo;
            result.EmpPhotoSize = employee?.EmpPhotoSize;
            var active = (user.Active == null || user.Active == true) ? true : false;
            result.Active = active;
            return result;
        }
        public int Login(string username, string password, Guid companyId, out LoginReturnModel modelReturn, PermissionInfo permissionInfo)
        {
            SearchResult ldapInfo = null;
            bool isAuthenticated = false;
            SysUserLevel userLevel = null;
            PermissionInfo info = null;

            SysUser sysUser = DataContext.Get(x => x.Username == username).FirstOrDefault();

            if (sysUser == null)
            {
                modelReturn = null;
                return -2;
            }
            if(sysUser.IsLdap == true)
            {
                // Check username are login using LDAP.
                isAuthenticated = CheckLdapInfo(username, password, out ldapInfo);
                if (isAuthenticated)
                {
                    if (!ValidateCompany(sysUser.Id, companyId))
                    {
                        modelReturn = null;
                        return -3;
                    }
                    if (!ValidateOffice(sysUser.Id))
                    {
                        modelReturn = null;
                        return -4;
                    }
                    SysUserLevel levelOffice = userLevelRepository.Get(lv => lv.UserId == sysUser.Id && lv.OfficeId != null)?.FirstOrDefault();
                    HandleState hs = UpdateUserInfoFromLDAP(ldapInfo, sysUser, password);

                    userLevel = detectSwitchOfficeDeptGroup(sysUser.Id, companyId, permissionInfo);
                    info = new PermissionInfo
                    {
                        OfficeID = userLevel.OfficeId,
                        DepartmentID = userLevel.DepartmentId != null ? (Int16)userLevel.DepartmentId : (Int16)0,
                        GroupID = userLevel.GroupId,
                        CompanyID = userLevel.CompanyId
                    };

                    modelReturn = SetLoginReturnModel(sysUser, userLevel, info);
                    modelReturn.companyId = companyId;

                    LogUserLogin(sysUser, companyId);

                    return 1;
                }
                else
                {
                    modelReturn = null;
                    return -2;
                }
            }
            else
            {
                if (BCrypt.Net.BCrypt.Verify(password, sysUser.Password))
                {
                    if (!ValidateCompany(sysUser.Id, companyId))
                    {
                        modelReturn = null;
                        return -3;
                    }
                    if (!ValidateOffice(sysUser.Id))
                    {
                        modelReturn = null;
                        return -4;
                    }

                    userLevel = detectSwitchOfficeDeptGroup(sysUser.Id, companyId, permissionInfo);
                    info = new PermissionInfo
                    {
                        OfficeID = userLevel.OfficeId,
                        DepartmentID = userLevel.DepartmentId != null ? (short)userLevel.DepartmentId : (short?)null,
                        GroupID = userLevel.GroupId,
                        CompanyID = userLevel.CompanyId
                    };

                    modelReturn = SetLoginReturnModel(sysUser, userLevel, info);
                    modelReturn.companyId = companyId;

                    LogUserLogin(sysUser, companyId);

                    return 1;

                }
                else
                {
                    modelReturn = null;
                    return -2;
                }
            }
        }

        private SysUserLevel detectSwitchOfficeDeptGroup(string userId, Guid companyId, PermissionInfo permissionInfo)
        {
            SysUserLevel userLevel = null;
            PermissionInfo info = null;

            // switch company
            if (permissionInfo == null)
            {
                userLevel = userLevelRepository.Get(lv => lv.UserId == userId && lv.OfficeId != null && lv.CompanyId == companyId)?.FirstOrDefault();
            }
            // switch office
            else if (permissionInfo.OfficeID != null && (permissionInfo.GroupID == null))
            {
                userLevel = userLevelRepository.Get(lv => lv.UserId == userId
                && lv.OfficeId == permissionInfo.OfficeID
                && lv.CompanyId == companyId
                )?.FirstOrDefault();
            }
            //switch group-dept
            else
            {
                userLevel = userLevelRepository.Get(lv => lv.UserId == userId
                && lv.OfficeId == permissionInfo.OfficeID
                && lv.CompanyId == companyId
                && lv.DepartmentId == permissionInfo.DepartmentID
                && lv.GroupId == permissionInfo.GroupID
                )?.FirstOrDefault();
            }

            return userLevel;
        }

        private bool CheckLdapInfo(string username, string password, out SearchResult ldapInfo)
        {
            bool isAuthenticated = false;
            LdapAuthentication Ldap = new LdapAuthentication();
            ldapInfo = null;
            foreach (var path in ldap.LdapPaths)
            {
                Ldap.Path = path;
                isAuthenticated = Ldap.IsAuthenticated(ldap.Domain, username, password);

                // if username login using LDAP -> get ldapInfo.
                if (isAuthenticated)
                {
                    ldapInfo = Ldap.GetNodeInfomation(ldap.Domain, username, password);
                    break;
                }
            }
            return isAuthenticated;
        }

        private LoginReturnModel SetLoginReturnModel(SysUser user, SysUserLevel levelOffice, PermissionInfo permissionInfo)
        {
            SysEmployee employee = employeeRepository.Get(x => x.Id == user.EmployeeId)?.FirstOrDefault();
            LoginReturnModel userInfo = new LoginReturnModel();

            userInfo.userName = user.Username;
            userInfo.email = employee?.Email;
            userInfo.idUser = user.Id;
            userInfo.status = true;
            //userInfo.NameEn = employee.EmployeeNameEn ?? "";
            //userInfo.NameVn = employee.EmployeeNameVn ?? "";
            //userInfo.BankAccountNo = employee.BankAccountNo ?? "";
            //userInfo.BankName = employee.BankName ?? "";
            //userInfo.Photo = employee.Photo ?? "";

            userInfo.message = "Login successfull !";

            if (permissionInfo == null)
            {
                userInfo.officeId = levelOffice.OfficeId;
                userInfo.departmentId = levelOffice.DepartmentId;
                userInfo.groupId = levelOffice?.GroupId;
            }
            else
            {
                userInfo.officeId = permissionInfo.OfficeID;
                userInfo.departmentId = permissionInfo.DepartmentID;
                userInfo.groupId = permissionInfo?.GroupID;
            }
            return userInfo;
        }

        private SysEmployee MapUserInfoFromLDAP(SearchResult ldapInfo)
        {
            var ldapProperties = ldapInfo.Properties;
            Guid? objId = null;
            if (ldapProperties["objectguid"].Count != 0)
            {
                objId = new Guid(ldapProperties["objectguid"][0] as byte[]);
            }
            var sysEmployee = new SysEmployee
            {
                Id = Guid.NewGuid().ToString(),
                Email = ldapProperties["mail"][0].ToString(),
                UserCreated = "admin",
                DatetimeCreated = DateTime.Now,
                EmployeeNameEn = ldapProperties["displayname"][0].ToString(),
                EmployeeNameVn = ldapProperties["name"][0].ToString(),
                Tel = ldapProperties["telephonenumber"].Count == 0 ? null : ldapProperties["telephonenumber"][0].ToString(),
                HomePhone = ldapProperties["mobile"].Count == 0 ? null : ldapProperties["mobile"][0].ToString(),
                LdapObjectGuid = objId
            };
            return sysEmployee;
        }
        private HandleState UpdateUserInfoFromLDAP(SearchResult ldapInfo, SysUser user, string password)
        {
            //LoginReturnModel modelReturn = null;

            var employee = employeeRepository.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
            var sysEmployee = MapUserInfoFromLDAP(ldapInfo);

            user.LdapObjectGuid = sysEmployee.LdapObjectGuid;
            employee.LdapObjectGuid = sysEmployee.LdapObjectGuid;
            employee.EmployeeNameEn = sysEmployee.EmployeeNameEn;
            employee.Email = sysEmployee.Email;

            // user.PasswordLdap = BCrypt.Net.BCrypt.HashPassword(password);

            DataContext.Update(user, x => x.Id == user.Id);
            var hs = employeeRepository.Update(sysEmployee, x => x.Id == employee.Id);
            //modelReturn = new LoginReturnModel { idUser = user.Id, userName = user.Username, email = sysEmployee.Email };
            return hs;
        }

        private string GetLocalIPAddress()
        {
            try
            {
                string ipv4 = string.Empty;
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4 = ip.ToString();
                    }
                }
                return ipv4;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string GetLocalComputerName()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                return host.HostName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private void LogUserLogin(SysUser user, Guid? workplaceId)
        {
            //IPHostEntry ipEntry = Dns.GetHostEntry("");
            string ComputerName = GetLocalComputerName();//ipEntry.HostName;
            var userLog = new SysUserLogModel
            {
                Ip = GetLocalIPAddress(),//ipEntry.AddressList[1].ToString(),
                ComputerName = ComputerName,
                LoggedInOn = DateTime.Now,
                UserId = user.Id,
                WorkPlaceId = workplaceId
            };
            HandleState hs = userLogService.Add(userLog);
        }

        private Boolean ValidateCompany(string userId, Guid companyId)
        {
            var levelCompany = userLevelRepository.Get(level => level.CompanyId == companyId && level.UserId == userId)?.FirstOrDefault();
            if (levelCompany == null)
            {
                return false;
            }
            return true;
        }

        private Boolean ValidateOffice(string userId)
        {
            var levelOffice = userLevelRepository.Get(lv => lv.UserId == userId && lv.OfficeId != null)?.FirstOrDefault();
            if (levelOffice == null)
            {
                return false;
            }
            return true;
        }

        public int ValidateAuthPartner(string username, string password, out LoginReturnModel modelReturn)
        {
            int result = -2;
            LoginReturnModel _modelReturn = new LoginReturnModel();
            SysUser sysUser = DataContext.Get(x => x.Username == username).FirstOrDefault();

            if (sysUser == null)
            {
                result = - 2;
            }

            // Check password
            if(BCrypt.Net.BCrypt.Verify(password, sysUser.Password))
            {
                result = 1;
                _modelReturn.idUser = sysUser.Id;
            }
            modelReturn = _modelReturn;
            return result;
        }
    }
}
