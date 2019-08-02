using AutoMapper;
using eFMS.API.Operation.Service.Contexts;
using eFMS.API.Common.Helpers;
using eFMS.API.Operation.DL.Common;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Service.Models;
using eFMS.API.Operation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace eFMS.API.Operation.DL.Services
{
    public class OpsTransactionSevice : RepositoryBase<OpsTransaction, OpsTransactionModel>, IOpsTransactionService
    {
        private readonly ICurrentUser currentUser;
        private readonly IDistributedCache cache;

        public OpsTransactionSevice(IContextBase<OpsTransaction> repository, IMapper mapper, ICurrentUser user, IDistributedCache distributedCache) : base(repository, mapper)
        {
            currentUser = user;
            cache = distributedCache;
        }
        
        public IQueryable<OpsTransaction> Get()
        {
            var clearanceCaching = RedisCacheHelper.GetObject<List<OpsTransaction>>(cache, Templates.OpsTransaction.NameCaching.ListName);
            IQueryable<OpsTransaction> opsTransactions = null;
            if (clearanceCaching == null)
            {
                opsTransactions = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.OpsTransaction.NameCaching.ListName, opsTransactions);
            }
            else
            {
                opsTransactions = clearanceCaching.AsQueryable();
            }
            return opsTransactions;
        }

        public OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var stageViewList = GetStageViewList(criteria).AsQueryable();
            var summaryStageView = GetJobSummaryStageView(criteria);

            rowsCount = (stageViewList.Count() > 0) ? stageViewList.Count() : 0;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                stageViewList = stageViewList.Skip((page - 1) * size).Take(size);
            }

            var result = new OpsTransactionResult
            {
                OpsTransactions = stageViewList,
                ToTalInProcessing = summaryStageView.Count > 0 ? summaryStageView[0].Processing : 0,
                ToTalFinish = summaryStageView.Count > 0 ? summaryStageView[0].Done : 0,
                TotalOverdued = summaryStageView.Count > 0 ? summaryStageView[0].Overdued : 0,
                TotalCanceled = summaryStageView.Count > 0 ? summaryStageView[0].Canceled : 0
            };
            return result;
        }

        private List<sp_GetStageViewList> GetStageViewList(OpsTransactionCriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("isSearchAll", criteria.IsSearchAll),
                SqlParam.GetParameter("isSearchAdvance", criteria.IsSearchEdvance),
                SqlParam.GetParameter("jobNo", criteria.JobNo),
                SqlParam.GetParameter("hwbNo", criteria.HwbNo),
                SqlParam.GetParameter("productService", criteria.ProductService),
                SqlParam.GetParameter("serviceMode", criteria.ServiceMode),
                SqlParam.GetParameter("shipmentMode", criteria.ShipmentMode),
                SqlParam.GetParameter("customerId", criteria.CustomerId),
                SqlParam.GetParameter("fieldOps", criteria.FieldOps),
                SqlParam.GetParameter("serviceDateFrom", criteria.ServiceDateFrom),
                SqlParam.GetParameter("serviceDateTo", criteria.ServiceDateTo),
                SqlParam.GetParameter("isLoadDefault", criteria.isLoadDefault)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetStageViewList>(parameters);
        }

        private List<sp_GetJobSummaryStageView> GetJobSummaryStageView(OpsTransactionCriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("isSearchAll", criteria.IsSearchAll),
                SqlParam.GetParameter("isSearchAdvance", criteria.IsSearchEdvance),
                SqlParam.GetParameter("jobNo", criteria.JobNo),
                SqlParam.GetParameter("hwbNo", criteria.HwbNo),
                SqlParam.GetParameter("productService", criteria.ProductService),
                SqlParam.GetParameter("serviceMode", criteria.ServiceMode),
                SqlParam.GetParameter("shipmentMode", criteria.ShipmentMode),
                SqlParam.GetParameter("customerId", criteria.CustomerId),
                SqlParam.GetParameter("fieldOps", criteria.FieldOps),
                SqlParam.GetParameter("serviceDateFrom", criteria.ServiceDateFrom),
                SqlParam.GetParameter("serviceDateTo", criteria.ServiceDateTo),
                SqlParam.GetParameter("isLoadDefault", criteria.isLoadDefault)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetJobSummaryStageView>(parameters);
        }
    }
}
