using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.System.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Common;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


namespace eFMS.API.System.DL.Services
{
    public class SysUserService : RepositoryBase<SysUser, SysUserModel>, ISysUserService
    {
        public SysUserService(IContextBase<SysUser> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState AddUser(SysUserAddModel model)
        {
  
            try
            {
                var sysUser = new SysUserModel();
                sysUser.Id = model.Id;
                sysUser.Username = model.Username;
                sysUser.Password = model.Password;
                sysUser.WorkPlaceId = model.WorkPlaceId;
                sysUser.UserGroupId = model.UserGroupId;
                sysUser.DatetimeCreated = DateTime.Now;
                sysUser.UserCreated = "Thor";
                sysUser.Inactive = false;
                sysUser.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
              
                DataContext.Add(sysUser);
                var hs = new HandleState();
                return hs;
            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex);
                return hs;
            }

        }

        public List<SysUserViewModel> GetAll()
        {
            var data = ((eFMSDataContext)DataContext.DC).SysUser.Join(((eFMSDataContext)DataContext.DC).SysEmployee, x => x.EmployeeId, y => y.Id, (x, y) => new { x, y });
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in data)
            {
                var model = mapper.Map<SysUserViewModel>(item.x);
                model.EmployeeNameEn = item.y.EmployeeNameEn;
                model.EmployeeNameVn = item.y.EmployeeNameVn;
                results.Add(model);
            }
            return results;
        }

        public SysUserViewModel GetUserById(string Id)
        {
            var query = (from user in ((eFMSDataContext)DataContext.DC).SysUser 
                         join employee in ((eFMSDataContext)DataContext.DC).SysEmployee on user.EmployeeId equals employee.Id
                         where user.Id == Id
                         select new { user,employee}).FirstOrDefault();
            if (query == null)
            {
                return null;
            }
            var result = mapper.Map<SysUserViewModel>(query.user);
            result.EmployeeNameEn = query.employee.EmployeeNameEn;
            result.EmployeeNameVn = query.employee.EmployeeNameVn;
            return result;
        }

        public List<vw_sysUser> GetUserWorkplace()
        {
            List<vw_sysUser> lvWorkspace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
            return lvWorkspace;
        }

        public LoginReturnModel Login(string username, string password)
        {
            LoginReturnModel userInfo = new LoginReturnModel();
            try
            {
                
                var employee = ((eFMSDataContext)DataContext.DC).SysEmployee.Where(x => x.Email == username).FirstOrDefault();

                var user = employee==null? DataContext.First(x => x.Username == username) : DataContext.First(x=>x.EmployeeId==employee.Id);
                if (user == null)
                {
                    throw new ApplicationException("Username or password incorrect !");
                }            

                bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(password, user.Password);               
                if (!isCorrectPassword)
                {
                    throw new ApplicationException("Username or password incorrect !");
                }               
              
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("EFMS-ITLCOPREFMS-ITLCOPREFMS-ITLCOPREFMS-ITLCOPREFMS-ITLCOPREFMS-ITLCOPREFMS-ITLCOPR");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);                
                
                userInfo.userName = user.Username;
                userInfo.email = employee == null ? ((eFMSDataContext)DataContext.DC).SysEmployee.First(x => x.Id == user.EmployeeId).Email : ((eFMSDataContext)DataContext.DC).SysEmployee.First(x => x.Id == employee.Id).Email;
                userInfo.token = tokenHandler.WriteToken(token);
                userInfo.status = true;
                userInfo.message = "Login successfull !";

                return userInfo;     
            }
            catch(Exception ex)
            {
                userInfo.status = false;
                userInfo.message = ex.Message;
                return userInfo;
            }
        }

   
    }
}
