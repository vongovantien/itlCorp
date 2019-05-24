using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
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
        private ISysUserApiService sysUserApi;

        public OpsTransactionService(IContextBase<OpsTransaction> repository, IMapper mapper, 
            ICatStageApiService stageApi,
            ICatPlaceApiService placeApi,
            ICatPartnerApiService partnerApi,
            ISysUserApiService userApi) : base(repository, mapper)
        {
            catStageApi = stageApi;
            catplaceApi = placeApi;
            catPartnerApi = partnerApi;
            sysUserApi = userApi;
        }

        public List<OpsTransactionModel> Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            List<OpsTransactionModel> results = null;
            var data = Query(criteria);
            rowsCount = data.Count();
            if (rowsCount == 0) return results;
            if (size > 1)
            {
                data = data.OrderByDescending(x => x.ModifiedDate);
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
                var stages = catStageApi.GetStages(null).Result.ToList();
                var query = (from tran in data
                             join stageAssign in ((eFMSDataContext)DataContext.DC).OpsStageAssigned on tran.Id equals stageAssign.JobId into grpStageAssigneds
                             from assinged in grpStageAssigneds.DefaultIfEmpty()
                             join stageData in stages on assinged.StageId equals stageData.Id into grpSatges
                             from stage in grpSatges.DefaultIfEmpty()
                             where assinged.IsCurrentStage == true
                             select new { tran, assinged, stage }
                             );
                results = new List<OpsTransactionModel>();
                foreach (var item in query)
                {
                    var trans = item.tran;
                    trans.CurrentStatus = item.assinged?.Status;
                    trans.CurentStageCode = item.stage?.StageNameEn;
                    results.Add(trans);
                }
            }
            return results;
        }

        public IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria)
        {
            List<OpsTransactionModel> results = new List<OpsTransactionModel>();
            
            var places = catplaceApi.GetPlaces().Result.ToList();
            var users = sysUserApi.GetUsers().Result.ToList();
            IQueryable<OpsTransaction> transactions = null;
            if (criteria.All == null)
            {
                transactions = DataContext.Get(x => (x.Mblno ?? "").IndexOf(criteria.Mblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.Hblno ?? "").IndexOf(criteria.Hblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.CustomerId == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                && (x.FieldOps == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                && ((x.ServiceDate ?? null) >= criteria.ServiceDateFrom || criteria.ServiceDateFrom == null)
                                && ((x.ServiceDate ?? null) <= criteria.ServiceDateTo || criteria.ServiceDateTo == null)
                            );
            }
            else
            {
                transactions = DataContext.Get(x => (x.Mblno ?? "").IndexOf(criteria.Mblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Hblno ?? "").IndexOf(criteria.Hblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.CustomerId == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                   || (x.FieldOps == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                   && ((x.ServiceDate ?? null) >= (criteria.ServiceDateFrom ?? null) && (x.ServiceDate ?? null) <= (criteria.ServiceDateTo ?? null))
                               );
            }
            var query = (from tran in transactions
                         //join stageAssign in ((eFMSDataContext)DataContext.DC).OpsStageAssigned on tran.Id equals stageAssign.JobId into grpStageAssigneds
                         //from assinged in grpStageAssigneds.DefaultIfEmpty()
                         //join stageData in stages on assinged.StageId equals stageData.Id into grpSatges
                         //from stage in grpSatges.DefaultIfEmpty()
                         join portOfLoading in places on tran.Pol equals portOfLoading.Id into grpPOL
                         from pol in grpPOL.DefaultIfEmpty()
                         join portOfDes in places on tran.Pod equals portOfDes.Id into grpPOD
                         from pod in grpPOD.DefaultIfEmpty()
                         select new { tran, pol, pod }
                         );
            if (query == null)
                return null;
            foreach(var item in query)
            {
                var transaction = mapper.Map<OpsTransactionModel>(item.tran);
                transaction.PODName = item.pod?.NameEn;
                transaction.POLName = item.pol?.NameEn;
                //transaction.CurentStageCode = item.stage?.Code;
                results.Add(transaction);
            }
            return results.AsQueryable();
        }
    }
}
