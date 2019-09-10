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
using System.Data.SqlClient;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionService : RepositoryBase<CsTransaction, CsTransactionModel>, ICsTransactionService
    {
        private readonly ICurrentUser currentUser;
        public CsTransactionService(IContextBase<CsTransaction> repository, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
        }

        public object AddCSTransaction(CsTransactionEditModel model)
        {
            try
            {
                model.TransactionType = DataTypeEx.GetType(model.TransactionTypeEnum);
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.Id = Guid.NewGuid();
                int countNumberJob = dc.CsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
                transaction.JobNo = GenerateID.GenerateJobID(Constants.SEF_SHIPMENT, countNumberJob);
                //transaction.UserCreated = "01";
                transaction.CreatedDate = transaction.ModifiedDate= DateTime.Now;
                transaction.Inactive = false;
                transaction.UserModified = transaction.UserCreated;

                var hsTrans = dc.CsTransaction.Add(transaction);
                var containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);
                if(containers != null)
                {
                    foreach (var container in containers)
                    {
                        container.Id = Guid.NewGuid();
                        container.Mblid = transaction.Id;
                        container.UserModified = transaction.UserCreated;
                        container.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Add(container);
                    }
                }
                if(model.CsTransactionDetails != null)
                {
                    foreach(var item in model.CsTransactionDetails)
                    {
                        var transDetail = mapper.Map<CsTransactionDetail>(item);
                        transDetail.Id = Guid.NewGuid();
                        transDetail.JobId = transaction.Id;
                        transDetail.Inactive = false;
                        transDetail.UserCreated = transaction.UserModified = transaction.UserCreated;  //ChangeTrackerHelper.currentUser;
                        transDetail.DatetimeCreated = transaction.ModifiedDate = DateTime.Now;
                        dc.CsTransactionDetail.Add(transDetail);
                        if (item.CsMawbcontainers == null) continue;
                        else
                        {
                            foreach (var x in item.CsMawbcontainers)
                            {
                                var houseCont = mapper.Map<CsMawbcontainer>(x);
                                x.Hblid = x.Id;
                                x.Mblid = Guid.Empty;
                                x.Id = Guid.NewGuid();
                                dc.CsMawbcontainer.Add(x);
                            }
                        }
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

        public bool CheckAllowDelete(Guid jobId)
        {
            var query = (from detail in ((eFMSDataContext)DataContext.DC).CsTransactionDetail
                         where detail.JobId == jobId
                         join surcharge in ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge on detail.Id equals surcharge.Hblid
                         where surcharge.CreditNo != null || surcharge.DebitNo != null || surcharge.Soano != null
                         select detail);
            if (query.Any())
            {
                return false;
            }
            return true;
        }

        public HandleState DeleteCSTransaction(Guid jobId)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = new HandleState();
            try
            {
                var itemToDelete = ((eFMSDataContext)DataContext.DC).CsTransaction.Find(jobId);
                ((eFMSDataContext)DataContext.DC).CsTransaction.Remove(itemToDelete);
                var containers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Mblid == jobId);
                ((eFMSDataContext)DataContext.DC).CsMawbcontainer.RemoveRange(containers);
                var details = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == jobId);
                foreach(var item in details)
                {
                    var houseContainers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Hblid == item.Id);
                    ((eFMSDataContext)DataContext.DC).CsMawbcontainer.RemoveRange(houseContainers);
                    var surcharges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Hblid == item.Id);
                    ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.RemoveRange(surcharges);
                }
                ((eFMSDataContext)DataContext.DC).CsTransactionDetail.RemoveRange(details);
                ((eFMSDataContext)DataContext.DC).SaveChanges();
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        public CsTransactionModel GetById(Guid id)
        {
            var data = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault(x => x.Id == id);
            if (data == null) return null;
            else
            {
                var result = mapper.Map<CsTransactionModel>(data);
                if (result.ColoaderId != null) result.SupplierName = ((eFMSDataContext)DataContext.DC).CatPartner.FirstOrDefault(x => x.Id == result.ColoaderId).PartnerNameEn;
                if (result.AgentId != null) result.AgentName = ((eFMSDataContext)DataContext.DC).CatPartner.FirstOrDefault(x => x.Id == result.AgentId).PartnerNameEn;
                if (result.Pod != null) result.PODName = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == result.Pod)?.NameEn;
                if (result.Pol != null) result.POLName = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == result.Pol)?.NameEn;
                return result;
            }
        }

        public List<object> GetListTotalHB(Guid JobId)
        {
            List<object> returnList = new List<object>();
            var housebills = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == JobId).ToList();
            foreach(var item in housebills)
            {
                var totalBuying = (decimal?)0;
                var totalSelling = (decimal?)0;
                var totalobh = (decimal?)0;
                var totallogistic = (decimal?)0;


                var totalBuyingUSD = (decimal?)0;
                var totalSellingUSD = (decimal?)0;
                var totalobhUSD = (decimal?)0;
                var totallogisticUSD = (decimal?)0;

                var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Hblid == item.Id).ToList();

                foreach (var c in charges)
                {
                    var exchangeRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == c.CurrencyId && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var UsdToVnd = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == c.ExchangeDate.Value.Date && x.CurrencyFromId == "USD" && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var rate = exchangeRate == null ? 1 : exchangeRate.Rate;
                    var usdToVndRate = UsdToVnd == null ? 1 : UsdToVnd.Rate;
                    if (c.Type.ToLower() == "buy")
                    {
                        totalBuying += c.Total * rate;
                        totalBuyingUSD += (totalBuying / usdToVndRate);
                    }
                    if (c.Type.ToLower()== "sell")
                    {
                        totalSelling += c.Total * rate;
                        totalSellingUSD +=(totalSelling / usdToVndRate);
                    }
                    if (c.Type.ToLower() == "obh")
                    {
                        totalobh += c.Total * rate;
                        totalobhUSD += (totalobh / usdToVndRate);
                    }
                }

                var totalVND = totalSelling - totalBuying - totallogistic;
                var totalUSD = totalSellingUSD - totalBuyingUSD - totallogisticUSD;
                var obj = new { item.Hwbno, totalVND, totalUSD };
                returnList.Add(obj);
            }
            return returnList;
        }

        public object ImportCSTransaction(CsTransactionEditModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var transaction = mapper.Map<CsTransaction>(model);
                transaction.Id = Guid.NewGuid();
                int countNumberJob = dc.CsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
                transaction.JobNo = GenerateID.GenerateJobID(Constants.SEF_SHIPMENT, countNumberJob);
                //transaction.UserCreated = "01";
                transaction.CreatedDate = transaction.ModifiedDate = DateTime.Now;
                transaction.UserModified = model.UserCreated;
                transaction.Inactive = false;
                var hsTrans = dc.CsTransaction.Add(transaction);
                List<CsMawbcontainer> containers = null;
                if (model.CsMawbcontainers.Count > 0)
                {
                    containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);

                    foreach (var container in containers)
                    {
                        container.Id = Guid.NewGuid();
                        container.Mblid = transaction.Id;
                        container.UserModified = transaction.UserCreated;
                        container.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Add(container);
                    }
                }
                else
                {
                    containers = dc.CsMawbcontainer.Where(x => x.Mblid == model.Id).ToList();
                    foreach (var container in containers)
                    {
                        container.ContainerNo = string.Empty;
                        container.SealNo = string.Empty;
                        container.MarkNo = string.Empty;
                        container.Id = Guid.NewGuid();
                        container.Mblid = transaction.Id;
                        container.UserModified = transaction.UserCreated;
                        container.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Add(container);
                    }
                }
                var detailTrans = dc.CsTransactionDetail.Where(x => x.JobId == model.Id);
                if (detailTrans != null)
                {
                    int countDetail = dc.CsTransactionDetail.Count(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month 
                                                                        && x.DatetimeCreated.Value.Year == DateTime.Now.Year
                                                                        && x.DatetimeCreated.Value.Day == DateTime.Now.Day);
                    string generatePrefixHouse = GenerateID.GeneratePrefixHousbillNo();

                    if(dc.CsTransactionDetail.Any(x => x.Hwbno.IndexOf(generatePrefixHouse, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        generatePrefixHouse = Constants.SEF_HBL
                            + GenerateID.GeneratePrefixHousbillNo();
                    }
                    foreach (var item in detailTrans)
                    {
                        var houseId = item.Id;
                        item.Id = Guid.NewGuid();
                        item.JobId = transaction.Id;
                        item.Hwbno = GenerateID.GenerateHousebillNo(generatePrefixHouse, countDetail);
                        countDetail = countDetail + 1;
                        item.Inactive = false;
                        item.UserCreated = transaction.UserCreated;  //ChangeTrackerHelper.currentUser;
                        item.DatetimeCreated = DateTime.Now;
                        dc.CsTransactionDetail.Add(item);
                        var houseContainers = dc.CsMawbcontainer.Where(x => x.Hblid == houseId);
                        if(houseContainers != null) { 
                            foreach (var x in houseContainers)
                            {
                                x.Id = Guid.NewGuid();
                                x.Hblid = item.Id;
                                x.Mblid = Guid.Empty;
                                x.ContainerNo = string.Empty;
                                x.SealNo = string.Empty;
                                x.MarkNo = string.Empty;
                                x.UserModified = transaction.UserCreated;
                                x.DatetimeModified = DateTime.Now;
                                dc.CsMawbcontainer.Add(x);
                            }
                        }
                        var charges = dc.CsShipmentSurcharge.Where(x => x.Hblid == houseId);
                        if(charges != null)
                        {
                            foreach(var charge in charges)
                            {
                                charge.Id = Guid.NewGuid();
                                charge.UserCreated = transaction.UserCreated;
                                charge.DatetimeCreated = DateTime.Now;
                                charge.Hblid = item.Id;
                                charge.Soano = null;
                                charge.Soaclosed = null;
                                charge.SoaadjustmentRequestor = null;
                                charge.SoaadjustmentRequestedDate = null;
                                charge.SoaadjustmentReason = null;
                                charge.UnlockedSoadirector = null;
                                charge.UnlockedSoadirectorDate = null;
                                charge.UnlockedSoadirectorStatus = null;
                                charge.UnlockedSoasaleMan = null;
                                dc.CsShipmentSurcharge.Add(charge);
                            }
                        }
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

        public List<CsTransactionModel> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var results = new List<CsTransactionModel>();
            var list = Query(criteria);
            if (list == null)
            {
                rowsCount = 0; return results;
            }
            var tempList = list.GroupBy(x => new { x.ID,
                x.BranchID,
                x.MAWB,
                x.JobNo,
                x.TypeOfService,
                x.ETD,
                x.ETA,
                x.MBLType,
                x.ColoaderID,
                x.BookingNo,
                x.ShippingServiceType,
                x.AgentID,
                x.AgentName,
                x.POL,
                x.POLName,
                x.POD,
                x.PODName,
                x.PaymentTerm,
                x.LoadingDate,
                x.RequestedDate,
                x.FlightVesselName,
                x.VoyNo,
                //x.FlightVesselConfirmedDate,
                x.ShipmentType,
                x.ServiceMode,
                x.Commodity,
                x.InvoiceNo,
                x.PONo,
                x.PersonIncharge,
                x.DeliveryPoint,
                x.RouteShipment,
                x.Notes,
                x.Locked,
                x.LockedDate,
                x.UserCreated,
                x.CreatedDate,
                x.ModifiedDate,
                x.Inactive,
                x.InactiveOn,
                x.SupplierName,
                x.CreatorName,
                x.SumCont,
                x.SumCBM
            }).Select(x => new CsTransactionModel { Id = x.Key.ID,
                BranchId = x.Key.BranchID,
                Mawb = x.Key.MAWB,
                JobNo = x.Key.JobNo,
                TypeOfService = x.Key.TypeOfService,
                Etd = x.Key.ETD,
                Eta = x.Key.ETA,
                Mbltype = x.Key.MBLType,
                ColoaderId = x.Key.ColoaderID,
                BookingNo = x.Key.BookingNo,
                ShippingServiceType = x.Key.ShippingServiceType,
                AgentId = x.Key.AgentID,
                AgentName = x.Key.AgentName,
                Pol = x.Key.POL,
                POLName = x.Key.POLName,
                Pod = x.Key.POD,
                PODName = x.Key.PODName,
                PaymentTerm = x.Key.PaymentTerm,
                LoadingDate = x.Key.LoadingDate,
                RequestedDate = x.Key.RequestedDate,
                FlightVesselName = x.Key.FlightVesselName,
                VoyNo = x.Key.VoyNo,
                ShipmentType = x.Key.ShipmentType,
                ServiceMode = x.Key.ServiceMode,
                Commodity = x.Key.Commodity,
                InvoiceNo = x.Key.InvoiceNo,
                Pono = x.Key.PONo,
                PersonIncharge = x.Key.PersonIncharge,
                DeliveryPoint = x.Key.DeliveryPoint,
                RouteShipment = x.Key.RouteShipment,
                Notes = x.Key.Notes,
                Locked = x.Key.Locked,
                UserCreated = x.Key.UserCreated,
                CreatedDate = x.Key.CreatedDate,
                ModifiedDate = x.Key.ModifiedDate,
                Inactive = x.Key.Inactive,
                SupplierName = x.Key.SupplierName,
                CreatorName = x.Key.CreatorName,
                SumCont = x.Key.SumCont,
                SumCBM = x.Key.SumCBM });
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
        public IQueryable<sp_GetTransaction> Query(CsTransactionCriteria criteria)
        {
            var transactionType = DataTypeEx.GetType(criteria.TransactionType);
            var list = GetView(transactionType);
            if (list.Count == 0) return null;
            var containers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer;
            
            var query = (from transaction in list
                         join container in containers on transaction.ID equals container.Mblid into containerTrans
                         from cont in containerTrans.DefaultIfEmpty()
                         select new { transaction, cont?.ContainerNo, cont?.SealNo });
            IQueryable<sp_GetTransaction> results = null;

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
                    ).OrderByDescending(x => x.transaction.ModifiedDate).ThenByDescending(x => x.transaction.CreatedDate);
            }
            else
            {
                //query = query.ToList();
                query = query.Where(x => ((x.transaction.JobNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.MAWB ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.HWBNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.SupplierName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.transaction.AgentName ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || ((x.transaction.CustomerID ?? "") == criteria.All || string.IsNullOrEmpty(criteria.All))
                             || ((x.transaction.NotifyPartyID ?? "") == criteria.All || string.IsNullOrEmpty(criteria.All))
                             || ((x.transaction.SaleManID ?? "") == criteria.All || string.IsNullOrEmpty(criteria.All))
                             || (x.SealNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                             || (x.ContainerNo ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                             && ((x.transaction.ETD ?? null) >= (criteria.FromDate ?? null) && (x.transaction.ETD ?? null) <= (criteria.ToDate ?? null))
                    );
                query = query.OrderByDescending(x => x.transaction.ModifiedDate).ThenByDescending(x => x.transaction.CreatedDate).AsQueryable();
            }
            return results = query.Select(x => x.transaction).Distinct().AsQueryable();
        }

        public HandleState UpdateCSTransaction(CsTransactionEditModel model)
        {
            try
            {
                //model.TransactionType = DataTypeEx.GetType(model.TransactionTypeEnum);
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var transaction = mapper.Map<CsTransaction>(model);
                //transaction.UserModified = "01";
                transaction.ModifiedDate = DateTime.Now;
                var hsTrans = DataContext.Update(transaction, x => x.Id == transaction.Id, false);
                if (hsTrans.Success)
                {
                    if(model.CsMawbcontainers != null)
                    {
                        var containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);

                        foreach (var container in containers)
                        {
                            if (container.Id == Guid.Empty)
                            {
                                container.Id = Guid.NewGuid();
                                container.Mblid = transaction.Id;
                                container.UserModified = transaction.UserModified;
                                container.DatetimeModified = DateTime.Now;
                                dc.CsMawbcontainer.Add(container);
                            }
                            else
                            {
                                container.Mblid = transaction.Id;
                                container.UserModified = transaction.UserModified;
                                container.DatetimeModified = DateTime.Now;
                                dc.CsMawbcontainer.Update(container);
                            }
                        }
                    }
                    dc.SaveChanges();
                }
                return hsTrans;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private List<sp_GetTransaction> GetView(string transactionType)
        {
            //string transactionType = "SeaFCLExport";
            var parameters = new[]{
                new SqlParameter(){ ParameterName="@transactionType", Value = transactionType }
            };
            //List<vw_csTransaction> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_csTransaction>();
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetTransaction>(parameters);
            return list;
        }
    }
}
