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

        public object AddCSTransaction(CsTransactionEditModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.Id = Guid.NewGuid();
                int countNumberJob = dc.CsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
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
                var result = new HandleState();
                return new { model = transaction, result };
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new { model = new object { }, result };
            }
        }

        public CsTransactionModel GetById(Guid id)
        {
            var data = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault(x => x.Id == id);
            return data != null ? mapper.Map<CsTransactionModel>(data): null;
        }

        public List<CsTransactionModel> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var results = new List<CsTransactionModel>();
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0; return results;
            }
            var tempList = list.Select(x => new CsTransactionModel {
                    Id = x.ID,
                    BranchId = x.BranchID,
                    JobNo = x.JobNo,
                    Mawb = x.MAWB,
                    TypeOfService = x.TypeOfService,
                    Etd = x.ETD,
                    Eta = x.ETA,
                    Mbltype = x.MAWB,
                    ColoaderId = x.ColoaderID,
                    BookingNo = x.BookingNo,
                    ShippingServiceType  = x.ShippingServiceType,
                    AgentId = x.AgentID,
                    Pol = x.POL,
                    Pod = x.POD,
                    PaymentTerm = x.PaymentTerm,
                    LoadingDate = x.LoadingDate,
                    RequestedDate = x.RequestedDate,
                    FlightVesselName = x.FlightVesselName,
                    VoyNo = x.VoyNo,
                    FlightVesselConfirmedDate = x.FlightVesselConfirmedDate,
                    ShipmentType = x.ShipmentType,
                    ServiceMode = x.ServiceMode,
                    Commodity = x.Commodity,
                    InvoiceNo = x.InvoiceNo,
                    Pono = x.PONo,
                    PersonIncharge = x.PersonIncharge,
                    DeliveryPoint = x.DeliveryPoint,
                    RouteShipment = x.RouteShipment,
                    Notes = x.Notes,
                    Locked = x.Locked,
                    LockedDate = x.LockedDate,
                    UserCreated = x.UserCreated,
                    CreatedDate = x.CreatedDate,
                    UserModified = x.UserCreated,
                    ModifiedDate = x.ModifiedDate,
                    Inactive = x.Inactive,
                    InactiveOn = x.InactiveOn,
                    SupplierName = x.SupplierName,
                    AgentName = x.AgentName,
                    PODName = x.PODName,
                    POLName = x.POLName,
                    CreatorName = x.CreatorName,
                    SumCont = x.SumCont,
                    SumCBM = x.SumCBM

            }).ToList().Distinct();
            rowsCount = tempList.Count();
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                tempList = tempList.Skip((page - 1) * size).Take(size);
                results = tempList.ToList();
            }
            return results;
        }

        public IQueryable<vw_csTransaction> Query(CsTransactionCriteria criteria)
        {
            var list = GetView();
            var containers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer;

            //var query = list.GroupJoin(containers,
            //    transaction => transaction.ID,
            //    container => container.Mblid,
            //    (transaction, container) => new { transaction, container })
            //    .SelectMany(x => x.container.DefaultIfEmpty(),
            //                (x, y) => new { x.transaction, container = y });
            var query = (from transaction in list
                         join container in containers on transaction.ID equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         select new { transaction, cont?.ContainerNo, cont?.SealNo });
            IQueryable<vw_csTransaction> results = null;

            if (criteria.All == null)
            {
                query = query.Where(x => (x.transaction.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.MAWB ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.transaction.AgentName ?? "").IndexOf(criteria.AgentName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.transaction.CustomerID ?? "") == criteria.CustomerID || string.IsNullOrEmpty(criteria.CustomerID))
                    && ((x.transaction.NotifyPartyID ?? "") == criteria.NotifyPartyID || string.IsNullOrEmpty(criteria.NotifyPartyID))
                    && ((x.transaction.SaleManID ?? "") == criteria.SaleManID || string.IsNullOrEmpty(criteria.SaleManID))
                    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                    && ((x.transaction.ETD ?? null) >= (criteria.FromDate ?? null))
                    && ((x.transaction.ETD ?? null) <= (criteria.ToDate ?? null))
                    ).OrderByDescending(x => x.transaction.CreatedDate).ThenByDescending(x => x.transaction.ModifiedDate);
                //results = list.Where(x => ((x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && (x.MAWB ?? "").IndexOf(criteria.MAWB ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && (x.HWBNo ?? "").IndexOf(criteria.HWBNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && (x.SupplierName ?? "").IndexOf(criteria.SupplierName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && (x.AgentName ?? "").IndexOf(criteria.AgentName ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && ((x.CustomerID ?? "") == criteria.CustomerID || string.IsNullOrEmpty(criteria.CustomerID))
                //    && ((x.NotifyPartyID ?? "") == criteria.NotifyPartyID || string.IsNullOrEmpty(criteria.NotifyPartyID))
                //    && ((x.SaleManID ?? "") == criteria.SaleManID || string.IsNullOrEmpty(criteria.SaleManID))
                //    && (x.SealNo ?? "").IndexOf(criteria.SealNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && (x.ContainerNo ?? "").IndexOf(criteria.ContainerNo ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //    && ((x.ETD ?? null) >= (criteria.FromDate ?? null))
                //    && ((x.ETD ?? null) <= (criteria.ToDate ?? null))
                //    )).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).AsQueryable();
            }
            else
            {
                //results = list.Where(x => ((x.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || (x.MAWB ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || (x.HWBNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || (x.SupplierName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || (x.AgentName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || ((x.CustomerID ?? "") == criteria.CustomerID || string.IsNullOrEmpty(criteria.CustomerID))
                //             || ((x.NotifyPartyID ?? "") == criteria.NotifyPartyID || string.IsNullOrEmpty(criteria.NotifyPartyID))
                //             || ((x.SaleManID ?? "") == criteria.SaleManID || string.IsNullOrEmpty(criteria.SaleManID))
                //             || (x.SealNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || (x.ContainerNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                //             || ((x.ETD ?? null) >= (criteria.FromDate ?? null) && (x.ETD ?? null) <= (criteria.ToDate ?? null))
                //    )).OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.ModifiedDate).AsQueryable();
                query = query.Where(x => ((x.transaction.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.MAWB ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.HWBNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.SupplierName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.AgentName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || ((x.transaction.CustomerID ?? "") == criteria.CustomerID || string.IsNullOrEmpty(criteria.CustomerID))
                             || ((x.transaction.NotifyPartyID ?? "") == criteria.NotifyPartyID || string.IsNullOrEmpty(criteria.NotifyPartyID))
                             || ((x.transaction.SaleManID ?? "") == criteria.SaleManID || string.IsNullOrEmpty(criteria.SaleManID))
                             || (x.SealNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.ContainerNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || ((x.transaction.ETD ?? null) >= (criteria.FromDate ?? null) && (x.transaction.ETD ?? null) <= (criteria.ToDate ?? null))
                    )).OrderByDescending(x => x.transaction.CreatedDate).ThenByDescending(x => x.transaction.ModifiedDate).AsQueryable();
            }
            return results = query.Select(x => x.transaction).Distinct().AsQueryable();
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
