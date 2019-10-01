﻿using AutoMapper;
using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Linq;
using eFMS.IdentityServer.DL.IService;
using eFMS.API.System.DL.ViewModels;
using eFMS.IdentityServer.Service.Contexts;
using eFMS.IdentityServer.DL.Models;
using System;
using eFMS.IdentityServer.DL.Helpers;
using eFMS.IdentityServer.DL.Infrastructure;
using Microsoft.Extensions.Options;
using System.DirectoryServices;
using System.Security.Cryptography;
using System.IO;

namespace eFMS.IdentityServer.DL.Services
{
    public class AuthenticateService : RepositoryBase<SysUser, SysUserModel>, IAuthenUserService
    {
        protected ISysUserLogService userLogService;
        readonly LDAPConfig ldap;
        IContextBase<SysEmployee> employeeRepository;
        public AuthenticateService(IContextBase<SysUser> repository, IMapper mapper,
            ISysUserLogService logService,
            IOptions<LDAPConfig> ldapConfig,
            IContextBase<SysEmployee> employeeRepo) : base(repository, mapper)
        {
            userLogService = logService;
            ldap = ldapConfig.Value;
            employeeRepository = employeeRepo;
        }

        public SysUserViewModel GetUserById(string id)
        {
            var data = ((eFMSDataContext)DataContext.DC).SysEmployee.Join(((eFMSDataContext)DataContext.DC).SysUser, x => x.Id, y => y.EmployeeId,
                (x, y) => new { x, y }).FirstOrDefault(x => x.y.Id == id);
            if (data == null) return null;
            var result = new SysUserViewModel();
            result.Id = data.y.Id;
            result.Username = data.y.Username;
            //result.UserGroupId = data.y.UserGroupId;
            result.EmployeeId = data.y.EmployeeId;
            result.WorkPlaceId = data.y.WorkPlaceId;
            result.RefuseEmail = data.y.RefuseEmail;
            result.LdapObjectGuid = data.y.LdapObjectGuid;
            result.DepartmentId = data.x.DepartmentId;
            result.EmployeeNameVn = data.x.EmployeeNameVn;
            result.EmployeeNameEn = data.x.EmployeeNameEn;
            result.Position = data.x.Position;
            result.Birthday = data.x.Birthday;
            result.ExtNo = data.x.ExtNo;
            result.Tel = data.x.Tel;
            result.HomePhone = data.x.HomePhone;
            result.HomeAddress = data.x.HomeAddress;
            result.Email = data.x.Email;
            result.Photo = data.x.Photo;
            result.EmpPhotoSize = data.x.EmpPhotoSize;
            var inActive = (data.y.Inactive == null || data.y.Inactive == true ) ? true : false;
            result.InActive = inActive;
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
        public int Login(string username, string password,out LoginReturnModel modelReturn)
        {
            LdapAuthentication Ldap = new LdapAuthentication();
            SearchResult ldapInfo = null;
            bool isAuthenticated = false;
            SysEmployee employee;
            foreach (var path in ldap.LdapPaths)
            {
                Ldap.Path = path;
                isAuthenticated = Ldap.IsAuthenticated(ldap.Domain, username, password);
                
                if (isAuthenticated)
                {
                    ldapInfo = Ldap.GetNodeInfomation(ldap.Domain, username, password);
                    break;
                }
            }
            var user = DataContext.Get(x => x.Username == username).FirstOrDefault();
            if (isAuthenticated)
            {
                if (user != null)
                {
                    if(BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        employee = employeeRepository.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
                        modelReturn = UpdateUserInfoFromLDAP(ldapInfo, user, false, employee);
                        //modelReturn = SetLoginReturnModel(user, employee);
                        LogUserLogin(user, true ? employee.WorkPlaceId.ToString() : null);
                        return 1;
                    }
                }
                user = new SysUser { Username = username, Password = password, UserCreated = "admin" };
                modelReturn = UpdateUserInfoFromLDAP(ldapInfo, user, true, null);
                LogUserLogin(user, true ? modelReturn.workplaceId?.ToString() : null);
                return 1;
            }
            if (user == null)
            {
                modelReturn = null;
                return -2;
            }
            employee = employeeRepository.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
            modelReturn = SetLoginReturnModel(user, employee);
            LogUserLogin(user, true ? employee.WorkPlaceId.ToString() : null);
            return 1;
        }
        private LoginReturnModel SetLoginReturnModel(SysUser user, SysEmployee employee)
        {
            var userInfo = new LoginReturnModel
            {
                userName = user.Username,
                email = employee?.Email,
                idUser = user.Id,
                workplaceId = employee?.Id,
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
        private LoginReturnModel UpdateUserInfoFromLDAP(SearchResult ldapInfo, SysUser user, bool isNew = false, SysEmployee employee = null)
        {
            LoginReturnModel modelReturn = null;
            var sysEmployee = MapUserInfoFromLDAP(ldapInfo);
            if(isNew == true)
            {
                var newUser = new SysUser { Id = user.Username, Password = BCrypt.Net.BCrypt.HashPassword(user.Password), EmployeeId = sysEmployee.Id, Username = user.Username };
                DataContext.Add(newUser);
                employeeRepository.Add(sysEmployee);
                modelReturn = new LoginReturnModel { idUser = newUser.Id, userName = newUser.Username, email = sysEmployee.Email };
            }
            else
            {
                user.LdapObjectGuid = sysEmployee.LdapObjectGuid;
                employee.LdapObjectGuid = sysEmployee.LdapObjectGuid;
                employee.EmployeeNameEn = sysEmployee.EmployeeNameEn;
                employee.Email = sysEmployee.Email;
                DataContext.Update(user, x => x.Id == user.Id);
                employeeRepository.Update(sysEmployee, x => x.Id == employee.Id);
                modelReturn = new LoginReturnModel { idUser = user.Id, userName = user.Username, email = sysEmployee.Email };
            }
            return modelReturn;
        }
        private void LogUserLogin(SysUser user, string workplaceId)
        {
            var userLog = new SysUserLogModel
            {
                LoggedInOn = DateTime.Now,
                UserId = user.Id,
                WorkPlaceId = workplaceId
            };
            userLogService.Add(userLog);
        }
    }
}
