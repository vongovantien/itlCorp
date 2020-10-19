using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using eFMS.API.System.DL.Common;
using Microsoft.Extensions.Caching.Distributed;
using eFMS.API.System.Service.Contexts;
using Microsoft.Extensions.Localization;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Globals;
using eFMS.API.Common;

namespace eFMS.API.System.DL.Services
{
    public class SysUserService : RepositoryBase<SysUser, SysUserModel>, ISysUserService
    {
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IContextBase<SysUserLevel> userlevelRepository;
        private readonly IDistributedCache cache;
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysEmployeeService sysEmployeeService;
        private readonly ICurrentUser currentUser;
        private readonly ISysCompanyService sysCompanyRepository;
        private readonly ISysOfficeService sysOfficeRepository;
        private readonly IContextBase<SysImage> imageRepository;
        private readonly ISysImageService sysImageService;



        public SysUserService(IContextBase<SysUser> repository, IMapper mapper,
            IContextBase<SysEmployee> employeeRepo,
            IContextBase<SysImage> imageRepo,
            IContextBase<SysUserLevel> userlevelRepo, IDistributedCache distributedCache, IStringLocalizer<SystemLanguageSub> localizer,
            ISysEmployeeService employeeService,
            ICurrentUser currUser,
            ISysCompanyService sysCompanyRepo,
            ISysOfficeService sysOfficeRepo,
            ISysImageService sysImageRepo) : base(repository, mapper)
        {
            employeeRepository = employeeRepo;
            userlevelRepository = userlevelRepo;
            cache = distributedCache;
            stringLocalizer = localizer;
            sysEmployeeService = employeeService;
            currentUser = currUser;
            sysCompanyRepository = sysCompanyRepo;
            sysOfficeRepository = sysOfficeRepo;
            imageRepository = imageRepo;
            sysImageService = sysImageRepo;
        }

        public IQueryable<SysUserViewModel> GetAll()
        {
            var users = DataContext.Get(x=>x.Active == true);
            var employees = employeeRepository.Get();
            var data = users.Join(employees, x => x.EmployeeId, y => y.Id, (x, y) => new { x, y });
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in data)
            {
                var model = mapper.Map<SysUserViewModel>(item.x);
                model.EmployeeNameEn = item.y.EmployeeNameEn;
                model.EmployeeNameVn = item.y.EmployeeNameVn;
                results.Add(model);
            }
            return results?.OrderBy(x=>x.Username).AsQueryable();
        }

        public SysUserViewModel GetUserById(string Id)
        {
            var user = DataContext.Get(x => x.Id == Id).FirstOrDefault();
            if (user == null)
            {
                return null;
            }
            var employee = employeeRepository.Get(x => x.Id == user.EmployeeId).FirstOrDefault();
            var result = mapper.Map<SysUserViewModel>(user);
            result.EmployeeNameEn = employee?.EmployeeNameEn;
            result.EmployeeNameVn = employee?.EmployeeNameVn;
            return result;
        }
        public IQueryable<SysUserViewModel> Paging(SysUserCriteria criteria, int page, int size, out int? rowsCount)
        {
            var users = DataContext.Get();
            var employees = employeeRepository.Get();
            var userLevels = userlevelRepository.Get();
            var companies = sysCompanyRepository.Get();
            var offices = sysCompanyRepository.Get();
            var datas = from u in users
                        join e in employees on u.EmployeeId equals e.Id into em
                        from e in em.DefaultIfEmpty()
                        select new { u, e };
            if (criteria.All == null)
            {
                if (criteria.Active != null)
                {
                    datas = datas.Where(x => (x.u.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) > -1
                       && (x.e.EmployeeNameEn ?? "").IndexOf(criteria.EmployeeNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                       && (x.e.EmployeeNameVn ?? "").IndexOf(criteria.EmployeeNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                       && (x.u.UserType ?? "").IndexOf(criteria.UserType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                       && (x.u.Active == criteria.Active)
                       );
                }
                else
                {
                    datas = datas.Where(x => (x.u.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) > -1
                              && (x.e.EmployeeNameEn ?? "").IndexOf(criteria.EmployeeNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                              && (x.e.EmployeeNameVn ?? "").IndexOf(criteria.EmployeeNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                              && (x.u.UserType ?? "").IndexOf(criteria.UserType ?? "", StringComparison.OrdinalIgnoreCase) > -1
                              && (x.u.Active == criteria.Active || criteria.Active == null)
                              );
                }
            }
            else
            {
                if (criteria.All == "active")
                {
                    criteria.Active = true;
                }
                if (criteria.All == "inactive")
                {
                    criteria.Active = false;
                }
                if (criteria.Active == null)
                {
                    if (criteria.All == "status")
                    {
                        datas = null;
                    }
                    else
                    {
                        datas = datas.Where(x => (
                    ((x.u.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                    || (x.e.EmployeeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    || (x.e.EmployeeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    || (x.u.UserType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                    ));
                    }

                }
                if (criteria.Active != null)
                {
                    datas = datas.Where(x => x.u.Active == criteria.Active);
                }
            }

            datas = datas?.OrderByDescending(x => x.u.DatetimeModified);
            rowsCount = datas?.Count();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                datas = datas?.Skip((page - 1) * size).Take(size);
            }
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            if (datas != null)
            {
                foreach (var item in datas)
                {
                    var model = mapper.Map<SysUserViewModel>(item.u);
                    model.EmployeeNameEn = item.e?.EmployeeNameEn;
                    model.EmployeeNameVn = item.e?.EmployeeNameVn;
                    model.Title = item.e?.Title;
                    results.Add(model);
                }
            }
            return results.AsQueryable();
        }

        public HandleState Delete(string id)
        {
            try
            {
                var item = DataContext.Get(x => x.Id == id).FirstOrDefault();
                var hs = DataContext.Delete(x => x.Id == id);
                if (hs.Success)
                {
                    hs = sysEmployeeService.Delete(x => x.Id == item.EmployeeId);
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }

        }

        public IQueryable<SysUserViewModel> Query(SysUserCriteria criteria)
        {
            var users = DataContext.Get();
            var employees = employeeRepository.Get();
            var datas = from u in users
                        join e in employees on u.EmployeeId equals e.Id into em
                        from e in em.DefaultIfEmpty()
                        select new { u,e};

            if (criteria.All == null)
            {
                datas = datas.Where(x => (x.u.Username ?? "").IndexOf(criteria.Username ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.e.EmployeeNameEn ?? "").IndexOf(criteria.EmployeeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.e.EmployeeNameVn ?? "").IndexOf(criteria.EmployeeNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.u.UserType ?? "").IndexOf(criteria.UserType ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                         && (x.e.Active == criteria.Active || criteria.Active == null));
            }
            else
            {
                if (criteria.All.Contains("Active"))
                {
                    criteria.Active = true;
                }
                if (criteria.All.Contains("InActive"))
                {
                    criteria.Active = false;
                }
                datas = datas.Where(x => (
                          ((x.u.Username ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                          || (x.e.EmployeeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.e.Title ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.u.UserType ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                          || (x.u.Active == criteria.Active)
                          ));
            }
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in datas)
            {
                var model = mapper.Map<SysUserViewModel>(item.u);
                model.EmployeeNameEn = item.e?.EmployeeNameEn;
                model.EmployeeNameVn = item.e?.EmployeeNameVn;
                results.Add(model);
            }
            return results.AsQueryable();
        }

        public IQueryable<SysUserViewModel> QueryPermission(SysUserCriteria criteria)
        {
            var users = DataContext.Get();
            var employees = employeeRepository.Get();
            var uslevel = userlevelRepository.Get();
            var data = from u in users
                       join l in uslevel on u.Id equals l.UserId
                       join e in employees on u.EmployeeId equals e.Id
                       where l.CompanyId.ToString() == criteria.CompanyId && u.Active == criteria.Active
                       select new  { u,e};
            List<SysUserViewModel> results = new List<SysUserViewModel>();
            foreach (var item in data)
            {
                var model = mapper.Map<SysUserViewModel>(item.u);
                model.EmployeeNameEn = item.e.EmployeeNameEn;
                model.EmployeeNameVn = item.e.EmployeeNameVn;
                results.Add(model);
            }
            if (results.Count > 0)
            {
                results = results.GroupBy(x => x.Username).Select(g => g.First()).ToList();
            }
            return results?.OrderBy(x => x.Username).AsQueryable();
        }

        public List<SysUserImportModel> CheckValidImport(List<SysUserImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            list.ForEach(item =>
            {
                //check empty username
                string userName = item.Username;
                item.UsernameValid = true;
                if (string.IsNullOrEmpty(userName))
                {
                    item.Username = stringLocalizer[SystemLanguageSub.MSG_USER_USERNAME_EMPTY];
                    item.IsValid = false;
                    item.UsernameValid = false;
                }
                else
                {
                    var isFound = DataContext.Get().Any(x => x.Username == userName);
                    if (isFound)
                    {
                        item.IsValid = false;
                        item.UsernameValid = false;
                        item.Username = stringLocalizer[SystemLanguageSub.MSG_USER_USERNAME_EXISTED, item.Username].Value;
                    }

                }
                //check empty Name EN
                string nameEN = item.EmployeeNameEn;
                item.EmployeeNameEnValid = true;
                if (string.IsNullOrEmpty(nameEN))
                {
                    item.EmployeeNameEn = stringLocalizer[SystemLanguageSub.MSG_USER_NAMEEN_EMPTY];
                    item.IsValid = false;
                    item.EmployeeNameEnValid = false;
                }

                //check empty Full Name
                string nameVN = item.EmployeeNameVn;
                item.EmployeeNameVnValid = true;
                if (string.IsNullOrEmpty(nameVN))
                {
                    item.EmployeeNameVn = stringLocalizer[SystemLanguageSub.MSG_USER_NAMEVN_EMPTY];
                    item.IsValid = false;
                    item.EmployeeNameVnValid = false;
                }
                //check empty and existed staff code
                string staffCode = item.StaffCode;
                item.StaffCodeValid = true;
                if (string.IsNullOrEmpty(staffCode))
                {
                    item.StaffCode = stringLocalizer[SystemLanguageSub.MSG_USER_STAFFCODE_EMPTY];
                    item.IsValid = false;
                    item.StaffCodeValid = false;
                }
                else
                {
                    var isFound = sysEmployeeService.Get().Any(x => x.StaffCode == staffCode);
                    if (isFound)
                    {
                        item.IsValid = false;
                        item.StaffCodeValid = false;
                        item.StaffCode = stringLocalizer[SystemLanguageSub.MSG_USER_STAFFCODE_EXISTED,item.StaffCode].Value;
                    }
                }

                ////chek empty title
                //string title = item.Title;
                //item.TitleValid = true;
                //if (string.IsNullOrEmpty(title))
                //{
                //    item.Title = stringLocalizer[SystemLanguageSub.MSG_USER_TITLE_EMPTY];
                //    item.IsValid = false;
                //    item.TitleValid = false;
                //}
                //check empty and valid User Type
                string userType = item.UserType;
                item.UserTypeValid = true;
                if (string.IsNullOrEmpty(userType))
                {
                    item.UserType = stringLocalizer[SystemLanguageSub.MSG_USER_USERTYPE_EMPTY];
                    item.IsValid = false;
                    item.UserTypeValid = false;
                }
                else if (!userType.Equals("Normal User") && !userType.Equals("Local Admin") && !userType.Equals("Super Admin"))
                {
                    item.UserType = stringLocalizer[SystemLanguageSub.MSG_USER_USERTYPE_NOTFOUND];
                    item.IsValid = false;
                    item.UserTypeValid = false;
                }

                //check empty and valid working status
                string workingStatus = item.WorkingStatus;
                item.WorkingStatusValid = true;
                //if (string.IsNullOrEmpty(workingStatus))
                //{
                //    item.WorkingStatus = stringLocalizer[SystemLanguageSub.MSG_USER_WORKINGSTATUS_EMPTY];
                //    item.IsValid = false;
                //    item.WorkingStatusValid = false;
                //}
                if(workingStatus != null)
                {
                    if (!workingStatus.Equals("Working") && !workingStatus.Equals("Maternity leave") && !workingStatus.Equals("Off"))
                    {
                        item.WorkingStatus = stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND];
                        item.IsValid = false;
                        item.WorkingStatusValid = false;
                    }
                }
                //check empty and valid status 
                string status = item.Status;
                item.StatusValid = true;
                //if (string.IsNullOrEmpty(status))
                //{
                //    item.Status = stringLocalizer[SystemLanguageSub.MSG_USER_STATUS_EMPTY];
                //    item.IsValid = false;
                //    item.StatusValid = false;
                //}
                if(status != null)
                {
                    if (!status.Equals("Active") && !status.Equals("Inactive"))
                    {
                        item.Status = string.Format(stringLocalizer[SystemLanguageSub.MSG_USER_STATUS_NOTFOUND], item.Status);
                        item.IsValid = false;
                        item.StatusValid = false;
                    }
                }
                //check empty and valid email
                string email = item.Email;
                item.EmailValid = true;
                if (string.IsNullOrEmpty(email))
                {
                    item.Email = stringLocalizer[SystemLanguageSub.MSG_USER_EMAIL_EMPTY];
                    item.IsValid = false;
                    item.EmailValid = false;
                }
                var checkDupStaffCode = list.GroupBy(x => x.StaffCode)
                                              .Where(t => t.Count() > 1)
                                              .Select(y => y.Key)
                                              .ToList();
                var checkDupUserName = list.GroupBy(x => x.Username)
                                          .Where(t => t.Count() > 1)
                                          .Select(y => y.Key)
                                          .ToList();
                if (checkDupStaffCode.Count > 0)
                {
                    item.IsValid = false;
                    item.StaffCode = stringLocalizer[SystemLanguageSub.MSG_USER_STAFFCODE_DUPLICATE, item.StaffCode].Value;
                    item.StaffCodeValid = false;
                }

                if (checkDupUserName.Count > 0)
                {
                    item.IsValid = false;
                    item.UsernameValid = false;
                    item.Username = stringLocalizer[SystemLanguageSub.MSG_USER_USERNAME_DUPLICATE, item.Username].Value;

                }

            });
            return list;
        }


        public HandleState Insert(SysUserModel sysUserModel)
        {
            try
            {
                return DataContext.Add(sysUserModel);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }

        }

        public HandleState Update(SysUserModel model)
        {
            try
            {
                var entity = mapper.Map<SysUser>(model);
                if (entity.Active == true)
                {
                    entity.InactiveOn = DateTime.Now;
                }
                var hs = DataContext.Update(entity, x => x.Id == model.Id);
                if (hs.Success)
                {
                    cache.Remove(Templates.SysBranch.NameCaching.ListName);
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState Import(List<SysUserViewModel> data)
        {
            try
            {

                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                List<SysUser> sysUsers = new List<SysUser>();
                List<SysEmployee> sysEmployees = new List<SysEmployee>();
                foreach (var item in data)
                {
                    var objUser = new SysUserModel();
                    var objEmployee = new SysEmployee();
                    objEmployee.Id = Guid.NewGuid().ToString();
                    objEmployee.DatetimeCreated = objEmployee.DatetimeModified = DateTime.Now;
                    objEmployee.EmployeeNameEn = item.EmployeeNameEn;
                    objEmployee.EmployeeNameVn = item.EmployeeNameVn;
                    objEmployee.Tel = item.Tel;
                    objEmployee.Title = item.Title;
                    objEmployee.StaffCode = item.StaffCode;
                    objEmployee.Email = item.Email;
                    sysEmployees.Add(objEmployee);
                    objUser.Username = item.Username;
                    objUser.UserType = item.UserType;
                    objUser.WorkingStatus = !string.IsNullOrEmpty(item.WorkingStatus)? item.WorkingStatus : "Working";
                    objUser.Active = item.Status?.ToLower() == "active" ? true : (item.Status?.ToLower() == "inactive" ? false : false);
                    objUser.EmployeeId = objEmployee.Id;
                    objUser.Id = Guid.NewGuid().ToString();
                    objUser.UserCreated = objUser.UserModified = currentUser.UserID;
                    objUser.Password = BCrypt.Net.BCrypt.HashPassword(Constants.PERMISSION_RANGE_OWNER);
                    objUser.Description = item.Description;
                    objUser.Password = SystemConstants.Password;
                    objUser.Password = BCrypt.Net.BCrypt.HashPassword(objUser.Password);
                    sysUsers.Add(objUser);
                }

                dc.SysEmployee.AddRange(sysEmployees);
                dc.SysUser.AddRange(sysUsers);
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public SysUserModel GetUserModelById(string id)
        {
            var result = DataContext.Get(x => x.Id == id).Select(y => new SysUserModel
            {
                Active = y.Active,
                CreditLimit = y.CreditLimit,
                CreditRate = y.CreditRate,
                DatetimeCreated = y.DatetimeCreated,
                DatetimeModified = y.DatetimeModified,
                Description = y.Description,
                EmployeeId = y.EmployeeId,
                //EmployeeNameVn = y.EmployeeNameVn,
                Id = y.Id,
                InactiveOn = y.InactiveOn,
                IsLdap = y.IsLdap,
                LdapObjectGuid = y.LdapObjectGuid,
                RefuseEmail = y.RefuseEmail,
                SysEmployeeModel = new SysEmployeeModel(),
                UserCreated = y.UserCreated,
                //UserCreatedName = y.UserCreatedName,
                UserModified = y.UserModified,
                //UserModifiedName = y.UserModifiedName,
                Username = y.Username,
                UserType = y.UserType,
                WorkingStatus = y.WorkingStatus,
                WorkPlaceId = y.WorkPlaceId
            }).FirstOrDefault();
            var userCreate = DataContext.Get(x => x.Id == result.UserCreated).FirstOrDefault();
            var userModified = DataContext.Get(x => x.Id == result.UserModified).FirstOrDefault();
            // Get employee
            var currEmployee = employeeRepository.Get(x => x.Id == result.EmployeeId).FirstOrDefault();
            result.EmployeeNameVn = currEmployee?.EmployeeNameVn;
            result.UserCreatedName = userCreate?.Username;
            result.UserModifiedName = userModified?.Username;
            result.SysEmployeeModel.EmployeeNameEn = currEmployee.EmployeeNameEn;
            result.SysEmployeeModel.Title = currEmployee.Title;
            result.SysEmployeeModel.Email = currEmployee.Email;
            result.SysEmployeeModel.BankAccountNo = currEmployee.BankAccountNo;
            result.SysEmployeeModel.BankName = currEmployee.BankName;
            result.SysEmployeeModel.Tel = currEmployee.Tel;
            result.SysEmployeeModel.StaffCode = currEmployee.StaffCode;
            // get avatar through last modified date.
            result.Avatar = currEmployee?.Photo;

            if (result == null)
            {
                return null;
            }
            else
            {
                return result;
            }
        }

        public HandleState UpdateProfile(UserProfileCriteria criteria)
        {
            if(criteria == null)
            {
                return new HandleState();
            }
            // set change value -> currUser

            var currUser = DataContext.Get(x => x.Id == currentUser.UserID).FirstOrDefault();
            currUser.Description = criteria.Description?.Trim();

            // set change value -> currEmployee by employeeId of currUser
            var currEmployee = employeeRepository.Get(y => y.Id == currUser.EmployeeId).FirstOrDefault();
            currEmployee.EmployeeNameEn = criteria.EmployeeNameEn?.Trim();
            currEmployee.EmployeeNameVn = criteria.EmployeeNameVn?.Trim();
            currEmployee.Title = criteria.Title?.Trim();
            currEmployee.Tel = criteria.Tel?.Trim();
            currEmployee.Email = criteria.Email?.Trim();
            currEmployee.BankAccountNo = criteria.BankAccountNo?.Trim();
            currEmployee.BankName = criteria.BankName?.Trim();

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(currUser, x => x.Id == currUser.Id);
                    if (hs.Success)
                    {
                        // upload Image
                        if (criteria.Avatar != null)
                        {
                            sysImageService.UploadImage(criteria.Avatar, "User", currUser.Id);
                            // get avatar through last modified date.
                            var image = imageRepository.Get(x => x.Folder == "User" && x.ObjectId == currentUser.UserID)
                                .OrderByDescending(y => y.DatetimeModified).FirstOrDefault();
                            currEmployee.Photo = image?.Url;
                        }
                        // update Employee
                        hs = employeeRepository.Update(currEmployee, y => y.Id == currEmployee.Id);
                    }

                    trans.Commit();
                    return hs;
                }
                catch(Exception ex)
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
    }
}
