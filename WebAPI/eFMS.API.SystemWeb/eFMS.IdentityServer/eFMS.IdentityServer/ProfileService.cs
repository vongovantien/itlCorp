using eFMS.API.System.DL.Models;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.Models;
using eFMS.IdentityServer.Service.Models;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using ITL.NetCore.Connection.EF;

namespace eFMS.IdentityServer
{
    public class ProfileService: IProfileService
    {
        readonly IAuthenUserService authenUserService;
        readonly ISysEmployeeService employeeService;
        private readonly IContextBase<SysUserLevel> userLevelRepository;
        private readonly IContextBase<SysCompany> sysCompanyRepository;
        private readonly IContextBase<SysOffice> sysOfficeRepository;

        public ProfileService(IAuthenUserService service, 
            ISysEmployeeService emService,
            IContextBase<SysUserLevel> userLevelRepo,
            IContextBase<SysCompany> sysCompanyRepo,
            IContextBase<SysOffice> sysOfficeRepo
            )
        {
            authenUserService = service;
            employeeService = emService;
            userLevelRepository = userLevelRepo;
            sysCompanyRepository = sysCompanyRepo;
            sysOfficeRepository = sysOfficeRepo;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var messageError = String.Empty;
            Claim companyClaim = context.Subject.Claims.Where<Claim>(claim => claim.Type.Equals("companyId")).FirstOrDefault();
            Claim officeClaim = context.Subject.Claims.Where<Claim>(claim => claim.Type.Equals("officeId")).FirstOrDefault();
            Claim departmentclaim = context.Subject.Claims.Where<Claim>(claim => claim.Type.Equals("departmentId")).FirstOrDefault();
            Claim groupClaim = context.Subject.Claims.Where<Claim>(claim => claim.Type.Equals("groupId")).FirstOrDefault();

            var subjectId = context.Subject.GetSubjectId();
            var user = authenUserService.GetUserById(subjectId);
            var employee = employeeService.First(x => x.Id == user.EmployeeId);
            SysCompany company = sysCompanyRepository.Get(x => x.Id.ToString() == companyClaim.Value.ToString())?.FirstOrDefault();
            SysOffice office = sysOfficeRepository.Get(x => x.Id.ToString() == officeClaim.Value)?.FirstOrDefault();

            var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Id, user.Id),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.PreferredUserName,user.Username),
                    new Claim(JwtClaimTypes.PhoneNumber,user.Tel??""),
                    new Claim("userName", user.Username),
                    new Claim("employeeId", employee.Id),
                    new Claim("photo", employee.Photo ?? ""),
                    new Claim("nameEn", employee.EmployeeNameEn ?? ""),
                    new Claim("nameVn", employee.EmployeeNameVn ?? ""),
                    new Claim("title", employee.Title ?? ""),
                    new Claim("code", employee.StaffCode ?? ""),
                    new Claim("bankAccountNo", employee.BankAccountNo ?? ""),
                    new Claim("bankName", employee.BankName ?? ""),
                    new Claim("bankOfficeAccountNoVnd", office?.BankAccountVnd ?? ""),
                    new Claim("bankOfficeAccountNoUsd", office?.BankAccountUsd ?? ""),
                    new Claim("bankCode", employee.BankCode?? ""),
                    new Claim("kbExchangeRate", company.KbExchangeRate != null ? company.KbExchangeRate.ToString() : "0"),
                    new Claim("internalCode", !string.IsNullOrEmpty(office.InternalCode) ? office.InternalCode: ""),
                    new Claim("CompanyCode", company.Code),
                    new Claim("OfficeCode", office.Code),
                    companyClaim,
                    officeClaim,
                    departmentclaim,
                    groupClaim,
                };

            context.IssuedClaims = claims;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            var user = authenUserService.GetUserById(context.Subject.GetSubjectId());
            context.IsActive = (user != null);// && !user.InActive;
            return Task.FromResult(0);
        }
    }
}
