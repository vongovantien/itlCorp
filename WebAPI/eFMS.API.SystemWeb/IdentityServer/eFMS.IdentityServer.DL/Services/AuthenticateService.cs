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
using System.Security.Cryptography;
using System.IO;
using System.Net;

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
            IContextBase<SysUser> sysUserRepo,
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
        private string Signature(string password)
        {
            AesManaged aes = new AesManaged();
            byte[] cipherBytes = Convert.FromBase64String("ITL-$EFMS-&SECRET_KEY001");
            byte[] rgbIV = Convert.FromBase64String("0000000000000000");
            using (var rijndaelManaged = new RijndaelManaged { Key = cipherBytes, IV = rgbIV, Mode = CipherMode.CBC })
            {
                rijndaelManaged.BlockSize = 128;
                rijndaelManaged.KeySize = 256;
                using (var memoryStream =
                       new MemoryStream(Convert.FromBase64String(password)))
                using (var cryptoStream =
                       new CryptoStream(memoryStream,
                           rijndaelManaged.CreateDecryptor(cipherBytes, rgbIV),
                           CryptoStreamMode.Read))
                {
                    return new StreamReader(cryptoStream).ReadToEnd();
                }
            }
        }
        public int Login(string username, string password, Guid companyId, out LoginReturnModel modelReturn, PermissionInfo permissionInfo)
        {
            LdapAuthentication Ldap = new LdapAuthentication();
            SearchResult ldapInfo = null;
            bool isAuthenticated = false;
            SysEmployee employee;

            var sysUser = DataContext.Get(x => x.Username == username).FirstOrDefault();
            if (sysUser == null)
            {
                modelReturn = null;
                return -2;
            }
            if (sysUser.IsLdap == true)
            {
                // Nếu có sysUser có password LDAP.
                if (sysUser.PasswordLdap != null)
                {
                    // Check password - passwordLDAP.
                    if (BCrypt.Net.BCrypt.Verify(password, sysUser.PasswordLdap))
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

                        // Get danh sách level office của user.
                        var levelOffice = userLevelRepository.Get(lv => lv.UserId == sysUser.Id && lv.CompanyId == companyId && lv.OfficeId != null)?.FirstOrDefault();

                        employee = employeeRepository.Get(x => x.Id == sysUser.EmployeeId)?.FirstOrDefault();
                        modelReturn = SetLoginReturnModel(sysUser, employee);
                        modelReturn.companyId = companyId;

                        if (permissionInfo == null)
                        {
                            modelReturn.officeId = levelOffice.OfficeId;
                            modelReturn.departmentId = levelOffice.DepartmentId;
                            modelReturn.groupId = levelOffice?.GroupId;
                        }
                        else
                        {
                            modelReturn.officeId = permissionInfo.OfficeID;
                            modelReturn.departmentId = permissionInfo.DepartmentID;
                            modelReturn.groupId = permissionInfo?.GroupID;
                        }

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
                    // Check username are login using LDAP.
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

                    if (isAuthenticated && sysUser.PasswordLdap == null)
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
                        var levelOffice = userLevelRepository.Get(lv => lv.UserId == sysUser.Id && lv.OfficeId != null)?.FirstOrDefault();
                        employee = employeeRepository.Get(x => x.Id == sysUser.EmployeeId).FirstOrDefault();
                                              
                        modelReturn = UpdateUserInfoFromLDAP(ldapInfo, sysUser, password, employee);
                        modelReturn.companyId = companyId;

                        if (permissionInfo == null)
                        {
                            modelReturn.officeId = levelOffice.OfficeId;
                            modelReturn.departmentId = levelOffice.DepartmentId;
                            modelReturn.groupId = levelOffice?.GroupId;
                        }
                        else
                        {
                            modelReturn.officeId = permissionInfo.OfficeID;
                            modelReturn.departmentId = permissionInfo.DepartmentID;
                            modelReturn.groupId = permissionInfo?.GroupID;
                        }
                        // Update Log
                        LogUserLogin(sysUser, companyId);
                        return 1;
                    }

                    if (!isAuthenticated)
                    {
                        modelReturn = null;
                        return -2;
                    }
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
                    var levelOffice = userLevelRepository.Get(lv => lv.UserId == sysUser.Id && lv.OfficeId != null && lv.CompanyId == companyId)?.FirstOrDefault();
                                   
                    employee = employeeRepository.Get(x => x.Id == sysUser.EmployeeId)?.FirstOrDefault();
                    modelReturn = SetLoginReturnModel(sysUser, employee);

                    modelReturn.companyId = companyId;
                    if(permissionInfo == null)
                    {
                        modelReturn.officeId = levelOffice.OfficeId;
                        modelReturn.departmentId = levelOffice.DepartmentId;
                        modelReturn.groupId = levelOffice?.GroupId;
                    }
                    else
                    {
                        modelReturn.officeId = permissionInfo.OfficeID;
                        modelReturn.departmentId = permissionInfo.DepartmentID;
                        modelReturn.groupId = permissionInfo?.GroupID;

                    }
                   

                    // Update Log
                    LogUserLogin(sysUser, companyId);
                    return 1;
                }
                else
                {
                    modelReturn = null;
                    return -2;
                }
            }

            modelReturn = null;
            return -2;
        }
        private LoginReturnModel SetLoginReturnModel(SysUser user, SysEmployee employee)
        {
            var userInfo = new LoginReturnModel
            {
                userName = user.Username,
                email = employee?.Email,
                idUser = user.Id,
                companyId = employee?.CompanyId,
                status = true,
                message = "Login successfull !"
            };
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
                HomePhone = ldapProperties["mobile"][0].ToString(),
                LdapObjectGuid = objId
            };
            return sysEmployee;
        }
        private LoginReturnModel UpdateUserInfoFromLDAP(SearchResult ldapInfo, SysUser user, string password, SysEmployee employee = null)
        {
            LoginReturnModel modelReturn = null;

            var sysEmployee = MapUserInfoFromLDAP(ldapInfo);

            user.LdapObjectGuid = sysEmployee.LdapObjectGuid;
            employee.LdapObjectGuid = sysEmployee.LdapObjectGuid;
            employee.EmployeeNameEn = sysEmployee.EmployeeNameEn;
            employee.Email = sysEmployee.Email;

            user.PasswordLdap = BCrypt.Net.BCrypt.HashPassword(password);

            DataContext.Update(user, x => x.Id == user.Id);
            employeeRepository.Update(sysEmployee, x => x.Id == employee.Id);
            modelReturn = new LoginReturnModel { idUser = user.Id, userName = user.Username, email = sysEmployee.Email };
            return modelReturn;
        }
        private void LogUserLogin(SysUser user, Guid? workplaceId)
        {
            IPHostEntry ipEntry = Dns.GetHostEntry("");
            string ComputerName = ipEntry.HostName;
            var userLog = new SysUserLogModel
            {
                Ip = ipEntry.AddressList[1].ToString(),
                ComputerName = ComputerName,
                LoggedInOn = DateTime.Now,
                UserId = user.Id,
                WorkPlaceId = workplaceId
            };
            userLogService.Add(userLog);
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

        
    }
}
