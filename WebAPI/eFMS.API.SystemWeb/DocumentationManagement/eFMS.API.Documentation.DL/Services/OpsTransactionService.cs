using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class OpsTransactionService : RepositoryBase<OpsTransaction, OpsTransactionModel>, IOpsTransactionService
    {
        //private ICatStageApiService catStageApi;
        //private ICatPlaceApiService catplaceApi;
        //private ICatPartnerApiService catPartnerApi;
        //private ISysUserApiService sysUserApi;

        public OpsTransactionService(IContextBase<OpsTransaction> repository, IMapper mapper) : base(repository, mapper)
        {
            //catStageApi = stageApi;
            //catplaceApi = placeApi;
            //catPartnerApi = partnerApi;
            //sysUserApi = userApi;
        }
        public override HandleState Add(OpsTransactionModel model)
        {
            model.Id = Guid.NewGuid();
            model.CreatedDate = DateTime.Now;
            model.UserCreated = "admin"; //currentUser.UserID;
            model.ModifiedDate = model.CreatedDate;
            model.UserModified = model.UserCreated;
            int countNumberJob = ((eFMSDataContext)DataContext.DC).OpsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
            model.JobNo = GenerateID.GenerateOPSJobID("LOG", countNumberJob);
            var entity = mapper.Map<OpsTransaction>(model);
            return DataContext.Add(entity);
        }

        public OpsTransactionModel GetDetails(Guid id)
        {
            var details = DataContext.Where(x => x.Id == id).FirstOrDefault();
            OpsTransactionModel OpsDetails = new OpsTransactionModel();
            OpsDetails = mapper.Map<OpsTransactionModel>(details);

            if (details != null)
            {
                var agent = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == details.AgentId).FirstOrDefault();
                OpsDetails.AgentName = agent == null ? null : agent.PartnerNameEn;

                var supplier = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == details.SupplierId).FirstOrDefault();
                OpsDetails.SupplierName = supplier == null ? null : supplier.PartnerNameEn; 
            }

            return OpsDetails;
        }

        public OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            rowsCount = data.Count();
            var totalProcessing = data.Count(x => x.CurrentStatus == Enum.GetName(typeof(JobStatus), JobStatus.Processing));
            var totalfinish = data.Count(x => x.CurrentStatus == Enum.GetName(typeof(JobStatus), JobStatus.Finish));
            var totalOverdued = data.Count(x => x.CurrentStatus == Enum.GetName(typeof(JobStatus), JobStatus.Overdued));
            var totalCanceled = data.Count(x => x.CurrentStatus == Enum.GetName(typeof(JobStatus), JobStatus.Canceled));
            if (rowsCount == 0) return null;
            if (size > 1)
            {
                data = data.OrderByDescending(x => x.ModifiedDate);
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            var results = new OpsTransactionResult
            {
                OpsTransactions = data,
                ToTalInProcessing = totalProcessing,
                ToTalFinish = totalfinish,
                TotalOverdued = totalOverdued,
                TotalCanceled = totalCanceled
            };
            return results;
        }
        public bool CheckAllowDelete(Guid jobId)
        {
            var query = (from detail in ((eFMSDataContext)DataContext.DC).OpsTransaction
                         where detail.Id == jobId
                         join surcharge in ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge on detail.Id equals surcharge.Hblid
                         where surcharge.Soano != null || surcharge.OtherSoa != null
                         select detail);
            if (query.Any())
            {
                return false;
            }
            return true;
        }
        public IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria)
        {
            var data = GetView().AsQueryable();
            if (data == null)
                return null;
            if (criteria.All == null)
            {
                data = data.Where(x => (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.HWBNO ?? "").IndexOf(criteria.Hblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.CustomerID == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                && (x.FieldOpsID == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                && ((x.ServiceDate ?? null) >= criteria.ServiceDateFrom || criteria.ServiceDateFrom == null)
                                && ((x.ServiceDate ?? null) <= criteria.ServiceDateTo || criteria.ServiceDateTo == null)
                            );
            }
            else
            {
                data = data.Where(x => (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.HWBNO ?? "").IndexOf(criteria.Hblno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.CustomerID == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                   || (x.FieldOpsID == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                   && ((x.ServiceDate ?? null) >= (criteria.ServiceDateFrom ?? null) && (x.ServiceDate ?? null) <= (criteria.ServiceDateTo ?? null))
                               );
            }
            List<OpsTransactionModel> results = new List<OpsTransactionModel>();
            results = mapper.Map<List<OpsTransactionModel>>(data);
            return results.AsQueryable();
        }
        private List<sp_GetOpsTransaction> GetView()
        {
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetOpsTransaction>(null);
            return list;
        }
    }
}
