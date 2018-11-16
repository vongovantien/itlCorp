using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.System.Service.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using eFMS.IdentityServer.DL.IService;

namespace eFMS.IdentityServer.DL.Services
{
    public class AuthenticateService : RepositoryBase<SysUser, SysUserModel>, IAuthenUserService
    {
        public AuthenticateService(IContextBase<SysUser> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public int Login(string username, string password,out LoginReturnModel modelReturn)
        {
            LoginReturnModel userInfo = new LoginReturnModel();
           
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

            return 1;          
        }
    }
}
