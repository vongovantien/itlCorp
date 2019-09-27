using AutoMapper;
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

namespace eFMS.IdentityServer.DL.Services
{
    public class AuthenticateService : RepositoryBase<SysUser, SysUserModel>, IAuthenUserService
    {
        protected ISysUserLogService userLogService;
        readonly LDAPConfig ldap;
        public AuthenticateService(IContextBase<SysUser> repository, IMapper mapper,
            ISysUserLogService logService,
            IOptions<LDAPConfig> ldapConfig) : base(repository, mapper)
        {
            userLogService = logService;
            ldap = ldapConfig.Value;
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

        public int Login(string username, string password,out LoginReturnModel modelReturn)
        {
            LoginReturnModel userInfo = new LoginReturnModel();

            LdapAuthentication Ldap = new LdapAuthentication();
            bool isAuthenticated = false;

            foreach (var path in ldap.LdapPaths)
            {
                Ldap.Path = path;
                isAuthenticated = Ldap.IsAuthenticated(ldap.Domain, username, password);
                if (isAuthenticated)
                    break;
            }
            var employee = ((eFMSDataContext)DataContext.DC).SysEmployee.Where(x => x.Email == username).FirstOrDefault();

            var user = employee == null ? DataContext.First(x => x.Username == username) : DataContext.First(x => x.EmployeeId == employee.Id);
            if (user == null)
            {
                modelReturn = null;
                return  -2;
            }

            bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!isCorrectPassword)
            {
                modelReturn = null;
                return -2;        
            }

            userInfo.userName = user.Username;
            userInfo.email = employee == null ? ((eFMSDataContext)DataContext.DC).SysEmployee.First(x => x.Id == user.EmployeeId).Email : ((eFMSDataContext)DataContext.DC).SysEmployee.First(x => x.Id == employee.Id).Email;
            userInfo.idUser = user.Id;
            userInfo.workplaceId = employee == null ? ((eFMSDataContext)DataContext.DC).SysEmployee.First(x => x.Id == user.EmployeeId).Id : ((eFMSDataContext)DataContext.DC).SysEmployee.First(x => x.Id == employee.Id).Id;
            userInfo.status = true;
            userInfo.message = "Login successfull !";
            modelReturn = userInfo;
            var userLog = new SysUserLogModel
            {
                LoggedInOn = DateTime.Now,
                UserId = user.Id,
                WorkPlaceId = (Guid?) new Guid(userInfo.workplaceId)
            };
            userLogService.Add(userLog);

            return 1;          
        }
    }
}
