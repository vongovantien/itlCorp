using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Common;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionService : RepositoryBase<CsTransaction, CsTransactionModel>, ICsTransactionService
    {
        public CsTransactionService(IContextBase<CsTransaction> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState AddCSTransaction(CsTransactionEditModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.Id = Guid.NewGuid();
                int countNumberJob = dc.CsTransaction.Count(x => x.CreatedDate.Value.Date == DateTime.Now.Date);
                transaction.JobNo = GenerateID.GenerateJobID("SEF", countNumberJob);
                transaction.UserCreated = "01";
                transaction.CreatedDate = DateTime.Now;
                var hsTrans = dc.CsTransaction.Add(transaction);
                var containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);
                if(model.CsTransactionDetails != null)
                {
                    foreach (var tranDetail in model.CsTransactionDetails)
                    {
                        var modelDetail = mapper.Map<CsTransactionDetail>(tranDetail);
                        tranDetail.Id = Guid.NewGuid();
                        tranDetail.JobId = transaction.Id;
                        tranDetail.UserCreated = "01";
                        tranDetail.DatetimeCreated = DateTime.Now;
                        dc.CsTransactionDetail.Add(tranDetail);

                        containers.ForEach(x =>
                        {
                            if (tranDetail.CsMawbcontainers.Any(y => y.Mblid == x.Mblid))
                            {
                                x.Hblid = tranDetail.Id;
                            }
                        });
                    }
                }
                if(containers != null)
                {
                    foreach (var container in containers)
                    {
                        container.Id = Guid.NewGuid();
                        container.Mblid = transaction.Id;
                        container.UserModified = "01";
                        container.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Add(container);
                    }
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public IQueryable<vw_csTransaction> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            rowsCount = list.Count();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip((page - 1) * size).Take(size);
            }
            return list;
        }

        public IQueryable<vw_csTransaction> Query(CsTransactionCriteria criteria)
        {
            var list = GetView();
            IQueryable<vw_csTransaction> results = null;
            if (criteria.All == null)
            {
                var query = list.Where(x => ((x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.MAWB ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.AgentName ?? "").IndexOf(criteria.AgentName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.CustomerID ?? "") == criteria.CustomerID || string.IsNullOrEmpty(criteria.CustomerID))
                    && ((x.NotifyPartyID ?? "") == criteria.NotifyPartyID || string.IsNullOrEmpty(criteria.NotifyPartyID))
                    && ((x.SaleManID ?? "") == criteria.SaleManID || string.IsNullOrEmpty(criteria.SaleManID))
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.ETD ?? null) >= (criteria.FromDate ?? null))
                    && ((x.ETD ?? null) <= (criteria.ToDate ?? null))
                    )).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).ToList();
                results = query.AsQueryable();
            }
            else
            {
                results = list.Where(x => ((x.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.MAWB ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.HWBNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.SupplierName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.AgentName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || ((x.CustomerID ?? "") == criteria.CustomerID || string.IsNullOrEmpty(criteria.CustomerID))
                             || ((x.NotifyPartyID ?? "") == criteria.NotifyPartyID || string.IsNullOrEmpty(criteria.NotifyPartyID))
                             || ((x.SaleManID ?? "") == criteria.SaleManID || string.IsNullOrEmpty(criteria.SaleManID))
                             || (x.SealNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.ContainerNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || ((x.ETD ?? null) >= (criteria.FromDate ?? null) && (x.ETD ?? null) <= (criteria.ToDate ?? null))
                    )).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).AsQueryable();
            }
            return results;
        }

        public HandleState UpdateCSTransaction(CsTransactionEditModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.UserModified = "01";
                transaction.ModifiedDate = DateTime.Now;
                var hsTrans = dc.CsTransaction.Update(transaction);
                //var containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);
                foreach (var container in model.CsMawbcontainers)
                {
                    container.Mblid = transaction.Id;
                    container.UserModified = "01";
                    container.DatetimeModified = DateTime.Now;
                    dc.CsMawbcontainer.Update(container);
                }
                foreach (var tranDetail in model.CsTransactionDetails)
                {
                    var modelDetail = mapper.Map<CsTransactionDetail>(tranDetail);
                    tranDetail.JobId = transaction.Id;
                    tranDetail.UserModified = "01";
                    tranDetail.DatetimeModified = DateTime.Now;
                    dc.CsTransactionDetail.Update(tranDetail);

                    //containers.ForEach(x =>
                    //{
                    //    if (tranDetail.CsMawbcontainers.Any(y => y.Mblid == x.Mblid))
                    //    {
                    //        x.Hblid = tranDetail.Id;
                    //    }
                    //});

                    foreach (var container in tranDetail.CsMawbcontainers)
                    {
                        container.Hblid = tranDetail.Id;
                        container.UserModified = "01";
                        container.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Update(container);
                    }
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private List<vw_csTransaction> GetView()
        {
            List<vw_csTransaction> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_csTransaction>();
            return lvCatPlace;
        }
    }
}
