using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Provider.Models.Criteria;
using eFMS.API.Provider.Services.IService;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class OpsTransactionService : RepositoryBase<OpsTransaction, OpsTransactionModel>, IOpsTransactionService
    {
        private ICatStageApiService catStageApi;
        private ICatPlaceApiService catplaceApi;
        private ICatPartnerApiService catPartnerApi;

        public OpsTransactionService(IContextBase<OpsTransaction> repository, IMapper mapper, 
            ICatStageApiService stageApi,
            ICatPlaceApiService placeApi,
            ICatPartnerApiService partnerApi) : base(repository, mapper)
        {
            catStageApi = stageApi;
            catplaceApi = placeApi;
            catPartnerApi = partnerApi;
        }

        public List<OpsTransactionModel> Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
        }

        public IQueryable<object> Query(OpsTransactionCriteria criteria)
        {
            var stages = catStageApi.GetStages(null).Result.ToList();
            var places = catplaceApi.GetPlaces().Result.ToList();
            var partners = catPartnerApi.GetPartners().Result.ToList();
            return null;
        }
    }
}
