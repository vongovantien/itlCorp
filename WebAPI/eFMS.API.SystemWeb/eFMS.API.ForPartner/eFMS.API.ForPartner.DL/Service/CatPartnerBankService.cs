using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.Service
{
    public class CatPartnerBankService : RepositoryBase<CatPartnerBank, CatPartnerBankModel>, ICatPartnerBankService
    {
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IOptions<AuthenticationSetting> configSetting;
        private readonly IStringLocalizer<LanguageSub> _stringLocalizer;
        private readonly ICurrentUser _currentUser;
        private readonly IActionFuncLogService _actionFuncLogService;
        
        public CatPartnerBankService(IContextBase<CatPartnerBank> repository,
            IContextBase<CatPartner> catParnerRepo,
            IContextBase<SysPartnerApi> sysPartnerApiRepo,
            IOptions<AuthenticationSetting> config,
            IMapper mapper,
            ICurrentUser currentUser,
            IStringLocalizer<LanguageSub> stringLocalizer,
            IActionFuncLogService actionFuncLogService) : base(repository, mapper)
        {
            catPartnerRepository = catParnerRepo;
            sysPartnerApiRepository = sysPartnerApiRepo;
            configSetting = config;
            _stringLocalizer = stringLocalizer;
            _currentUser = currentUser;
            _actionFuncLogService = actionFuncLogService;
        }

        public async Task<HandleState> UpdatePartnerBankInfoSyncStatus(BankStatusUpdateModel model, string apiKey)
        {

            var hs = new HandleState();

            ICurrentUser currentUser = await SetCurrentUserPartner(_currentUser, apiKey);
            currentUser.UserID = _currentUser.UserID;
            currentUser.GroupId = _currentUser.GroupId;
            currentUser.DepartmentId = _currentUser.DepartmentId;
            currentUser.OfficeID = _currentUser.OfficeID;
            currentUser.CompanyID = _currentUser.CompanyID;
            currentUser.Action = "UpdateBankInfoSyncStatus";

            var partner = await catPartnerRepository.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefaultAsync();
            if (partner == null)
            {
                return new HandleState(false, (object)"Thông tin partner không tồn tại");
            }

            var listBank = await DataContext.GetAsync(x => x.PartnerId == partner.Id);
            var errorsMessage = model.BankInfo
                .Where(item => !listBank.Any(bank => bank.BankAccountNo == item.BankAccountno))
                .Select(item => item.BankAccountno);

            if (errorsMessage.Any())
            {
                return new HandleState(false, (object)$"{string.Join(", ", errorsMessage)} không tồn tại trong hệ thống");
            }

            foreach (var item in model.BankInfo)
            {
                var bank = listBank.FirstOrDefault(x => x.BankAccountNo == item.BankAccountno);
                bank.ApproveDescription = item.Description;
                bank.ApproveStatus = item.ApproveStatus;
                hs = DataContext.Update(bank, x => x.Id == bank.Id, false);
            }
            hs = DataContext.SubmitChanges();
            return hs;
        }

        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if (partnerAPiInfo != null && partnerAPiInfo.Active == true)
            {
                isValid = true;
            }
            return isValid;
        }

        public bool ValidateHashString(object body, string apiKey, string hash)
        {
            bool valid = false;
            if (body != null)
            {
                string bodyString = apiKey + configSetting.Value.PartnerShareKey;

                string eFmsHash = Md5Helper.CreateMD5(bodyString.ToLower());

                if (eFmsHash.ToLower() == hash.ToLower())
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }
            }

            return valid;
        }

        private async Task<ICurrentUser> SetCurrentUserPartner(ICurrentUser currentUser, string apiKey)
        {
            SysPartnerApi partnerApi = await sysPartnerApiRepository.Where(x => x.ApiKey == apiKey).FirstOrDefaultAsync();

            currentUser.UserID = (partnerApi != null) ? partnerApi.UserId.ToString() : Guid.Empty.ToString();
            currentUser.GroupId = 0;
            currentUser.DepartmentId = 0;
            currentUser.OfficeID = Guid.Empty;
            currentUser.CompanyID = partnerApi?.CompanyId ?? Guid.Empty;

            return currentUser;
        }
    }
}
