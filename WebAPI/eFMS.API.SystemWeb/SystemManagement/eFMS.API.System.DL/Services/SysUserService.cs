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
            var data = Get().ToList();
            var results = mapper.Map<List<SysUserViewModel>>(data);
            return results;
        }

        public List<vw_sysUser> GetUserWorkplace()
        {
            List<vw_sysUser> lvWorkspace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_sysUser>();
            return lvWorkspace;
        }

        public LoginModel Login(string username, string password)
        {
            LoginModel userInfo = new LoginModel();
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
                //user.Token = tokenHandler.WriteToken(token);
                
                userInfo.userName = user.Username;
                userInfo.token = tokenHandler.WriteToken(token);
                userInfo.success = true;
                
                return userInfo;     
            }
            catch(Exception ex)
            {
                userInfo.success = false;
                userInfo.error = ex.Message;
                return userInfo;
            }
        }
    }
}
