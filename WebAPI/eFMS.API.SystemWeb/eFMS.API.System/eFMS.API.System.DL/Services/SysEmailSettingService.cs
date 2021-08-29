using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Linq;


namespace eFMS.API.System.DL.Services
{
    public class SysEmailSettingService : RepositoryBase<SysEmailSetting, SysEmailSettingModel>, ISysEmailSettingService
    {
        private readonly IDistributedCache cache;
        private readonly IContextBase<SysCompany> sysBuRepository;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUserLevel> sysLevelRepository;
        private readonly IContextBase<SysUser> sysUserRepo;

        public SysEmailSettingService(
            IContextBase<SysEmailSetting> repository,
            IMapper mapper,
            IDistributedCache distributedCache,
            IContextBase<SysUserLevel> userLevelRepo,
            IContextBase<SysUser> sysUser,
            ICurrentUser icurrentUser) : base(repository, mapper)
        {
            cache = distributedCache;
            currentUser = icurrentUser;
            sysUserRepo = sysUser;
        }

        public HandleState AddEmailSetting(SysEmailSettingModel SysEmailSetting)
        {
            try
            {
                SysEmailSetting.CreateDate = DateTime.Now;
                SysEmailSetting.ModifiedDate = DateTime.Now;
                SysEmailSetting.UserCreated = currentUser.UserID;
                SysEmailSetting.UserModified = currentUser.UserID;
                var modelAdd = mapper.Map<SysEmailSetting>(SysEmailSetting);
                return DataContext.Add(modelAdd);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public IQueryable<SysEmailSetting> GetEmailSettings()
        {
            var lstEmailSetting = DataContext.Get();
            return lstEmailSetting?.OrderBy(x => x.ModifiedDate).AsQueryable();
        }

        public IQueryable<SysEmailSetting> GetEmailSettingsByDebtId(int id)
        {
            var EmailSetting = DataContext.Get(x => x.DeptId == id);
            return EmailSetting.AsQueryable();
        }
        public SysEmailSettingModel GetEmailSettingById(int id)
        {
            var data = new SysEmailSettingModel();
            var EmailSetting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (EmailSetting != null)
            {
                data = mapper.Map<SysEmailSettingModel>(EmailSetting);
                data.UserNameCreated = sysUserRepo.Get(x => x.Id == EmailSetting.UserCreated).FirstOrDefault()?.Username;
                data.UserNameModified = sysUserRepo.Get(x => x.Id == EmailSetting.UserModified).FirstOrDefault()?.Username;
            }
            return data;
        }

        public HandleState UpdateEmailInfo(SysEmailSettingModel model)
        {
            try
            {
                var entity = mapper.Map<SysEmailSetting>(model);
                var emailCurrent = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                entity.ModifiedDate = DateTime.Now;
                entity.UserModified = currentUser.UserID;
                var hs = DataContext.Update(entity, x => x.Id == model.Id);
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState DeleteEmailSetting(int id)
        {
            try
            {
                var hs = DataContext.Delete(x => x.Id == id);
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public bool CheckExistsEmailInDept(SysEmailSettingModel model)
        {
            var isExists = false;
            isExists = DataContext.Get(x => x.DeptId == model.DeptId &&
              x.Id == model.Id).Any();
            return isExists;
        }

        public bool CheckValidEmail(SysEmailSettingModel model)
        {
            var isExists = true;
            if (string.IsNullOrEmpty(model.EmailType) || string.IsNullOrEmpty(model.EmailInfo)){
                isExists = false;
            }
            return isExists;
        }
    }
}

