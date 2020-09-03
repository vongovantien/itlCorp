using eFMS.API.ForPartner.Service.Models;
using eFMS.API.ForPartner.DL.Models;
using ITL.NetCore.Connection.BL;
using eFMS.API.ForPartner.DL.IService;
using System;
using ITL.NetCore.Connection.EF;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.ForPartner.DL.Models.Criteria;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccAccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IHostingEnvironment environment;
        private readonly IContextBase<CatUnit> catUnitRepository;


        public AccAccountingManagementService(
            IContextBase<AccAccountingManagement> repository,
            IContextBase<SysPartnerApi> sysPartnerApiRep,
            IHostingEnvironment env,
            IMapper mapper,
            ICurrentUser cUser,
            IContextBase<CatUnit> catUnitRepo
            ) : base(repository, mapper)
        {
            currentUser = cUser;
            sysPartnerApiRepository = sysPartnerApiRep;
            environment = env;
            catUnitRepository = catUnitRepo;
        }

        public AccAccountingManagementModel GetById(Guid id)
        {
            AccAccountingManagement accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagementModel result = mapper.Map<AccAccountingManagementModel>(accounting);
            return result;
        }

        public ChargeOfAcctMngtResult GetChargeInvoice(SearchAccMngtCriteria dataSearch)
        {
        }

        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if(partnerAPiInfo != null && partnerAPiInfo.Active == true)
            {
                isValid = true;
            }
            return isValid;
        }
    }
}
