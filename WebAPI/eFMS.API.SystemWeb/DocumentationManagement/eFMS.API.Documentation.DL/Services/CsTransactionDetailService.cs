using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionDetailService : RepositoryBase<CsTransactionDetail, CsTransactionDetailModel>, ICsTransactionDetailService
    {
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CsMawbcontainer> csMawbcontainerRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<SysUser> sysUserRepo;
        readonly IContextBase<CatUnit> catUnitRepo;
        readonly IContextBase<CatCommodity> catCommodityRepo;
        readonly IContextBase<CatSaleman> catSalemanRepo;

        readonly IContextBase<CsShipmentSurcharge> surchareRepository;

        public CsTransactionDetailService(IContextBase<CsTransactionDetail> repository,
            IMapper mapper,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsMawbcontainer> csMawbcontainer,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatPlace> catPlace,
            IContextBase<SysUser> sysUser,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatCommodity> catCommodity,
            IContextBase<CatSaleman> catSaleman,
            IContextBase<CsShipmentSurcharge> surchareRepo) : base(repository, mapper)
        {
            csTransactionRepo = csTransaction;
            csMawbcontainerRepo = csMawbcontainer;
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
            sysUserRepo = sysUser;
            catUnitRepo = catUnit;
            catCommodityRepo = catCommodity;
            surchareRepository = surchareRepo;
            catSalemanRepo = catSaleman;
        }

        #region -- INSERT & UPDATE HOUSEBILLS --
        public HandleState AddTransactionDetail(CsTransactionDetailModel model)
        {
            var detail = mapper.Map<CsTransactionDetail>(model);
            detail.Id = Guid.NewGuid();
            detail.UserModified = detail.UserCreated;
            detail.DatetimeModified = detail.DatetimeCreated = DateTime.Now;
            detail.Active = true;
            detail.SailingDate = DateTime.Now;
            try
            {
                var hs = DataContext.Add(detail);
                if (hs.Success)
                {
                    foreach (var x in model.CsMawbcontainers)
                    {
                        var cont = mapper.Map<CsMawbcontainerModel>(x);
                        cont.Id = Guid.NewGuid();
                        cont.Hblid = detail.Id;
                        cont.Mblid = Guid.Empty;
                        cont.UserModified = detail.UserModified;
                        cont.DatetimeModified = DateTime.Now;
                        //((eFMSDataContext)DataContext.DC).CsMawbcontainer.Add(cont);
                        var hsContainer = csMawbcontainerRepo.Add(cont);
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public HandleState UpdateTransactionDetail(CsTransactionDetailModel model)
        {
            try
            {
                var hb = DataContext.Where(x => x.Id == model.Id).FirstOrDefault();
                if (hb == null)
                {
                    new HandleState("Housebill not found !");
                }
                hb.UserModified = ChangeTrackerHelper.currentUser;
                hb.DatetimeModified = DateTime.Now;
                model.SailingDate = DateTime.Now;
                hb = mapper.Map<CsTransactionDetail>(model);
                var isUpdateDone = DataContext.Update(hb, x => x.Id == hb.Id);
                if (isUpdateDone.Success)
                {
                    if (model.CsMawbcontainers.Count > 0)
                    {
                        //var listConts = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Hblid == hb.Id).ToList();
                        var listConts = csMawbcontainerRepo.Get(x => x.Hblid == hb.Id).ToList();
                        foreach (var item in listConts)
                        {
                            var isExist = model.CsMawbcontainers.Where(x => x.Id == item.Id).FirstOrDefault();
                            if (isExist == null)
                            {
                                //((eFMSDataContext)DataContext.DC).CsMawbcontainer.Remove(item);
                                var hsContainerDetele = csMawbcontainerRepo.Delete(x => x.Id == item.Id);
                            }
                        }

                        foreach (var item in model.CsMawbcontainers)
                        {
                            //var cont = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Id == item.Id).FirstOrDefault();               
                            var cont = mapper.Map<CsMawbcontainer>(item);

                            if (cont.Id == Guid.Empty)
                            {
                                cont.Id = Guid.NewGuid();
                                cont.Hblid = hb.Id;
                                cont.UserModified = hb.UserModified;
                                cont.DatetimeModified = DateTime.Now;
                                //((eFMSDataContext)DataContext.DC).CsMawbcontainer.Add(cont);
                                var hsContainerAdd = csMawbcontainerRepo.Add(cont);
                            }
                            else
                            {
                                cont.Hblid = hb.Id;
                                cont.UserModified = hb.UserModified;
                                cont.DatetimeModified = DateTime.Now;
                                //((eFMSDataContext)DataContext.DC).CsMawbcontainer.Update(cont);
                                var hsContainerUpdate = csMawbcontainerRepo.Update(cont, x => x.Id == cont.Id);
                            }
                        }                        
                    }
                    else
                    {
                        //var conts = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Hblid == hb.Id).ToList();
                        var conts = csMawbcontainerRepo.Get(x => x.Hblid == hb.Id).ToList();
                        foreach (var item in conts)
                        {
                            //((eFMSDataContext)DataContext.DC).CsMawbcontainer.Remove(item);
                            var hsContainerDetele = csMawbcontainerRepo.Delete(x => x.Id == item.Id);
                        }
                    }
                }
                //((eFMSDataContext)DataContext.DC).SaveChanges();
                return isUpdateDone;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }
        #endregion -- INSERT & UPDATE HOUSEBILLS --

        public CsTransactionDetailModel GetHbDetails(Guid JobId, Guid HbId)
        {
            var listHB = GetByJob(new CsTransactionDetailCriteria { JobId = JobId });
            var returnHB = listHB.Where(x => x.Id == HbId).FirstOrDefault();
            return returnHB;

        }

        public List<CsTransactionDetailModel> GetByJob(CsTransactionDetailCriteria criteria)
        {
            var results = QueryDetail(criteria).ToList();
            //var containers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer
            //    .Join(((eFMSDataContext)DataContext.DC).CatUnit,
            //        container => container.ContainerTypeId,
            //        unit => unit.Id, (container, unit) => new { container, unit })
            //        .ToList();
            var containers = (from container in ((eFMSDataContext)DataContext.DC).CsMawbcontainer
                              join unit in ((eFMSDataContext)DataContext.DC).CatUnit on container.ContainerTypeId equals unit.Id
                              join unitMeasure in ((eFMSDataContext)DataContext.DC).CatUnit on container.UnitOfMeasureId equals unitMeasure.Id into grMeasure
                              from measure in grMeasure.DefaultIfEmpty()
                              join packageType in ((eFMSDataContext)DataContext.DC).CatUnit on container.PackageTypeId equals packageType.Id into grPackage
                              from packType in grPackage.DefaultIfEmpty()
                              join commodity in ((eFMSDataContext)DataContext.DC).CatCommodity on container.CommodityId equals commodity.Id into grCommodity
                              from commo in grCommodity.DefaultIfEmpty()
                              select new
                              {
                                  container,
                                  unit,
                                  UnitOfMeasureName = measure.UnitNameEn,
                                  PackageTypeName = packType.UnitNameEn,
                                  CommodityName = commo.CommodityNameEn
                              });

            if (containers.Count() == 0) return results;
            results.ForEach(detail =>
            {
                //detail.ContainerNames = string.Empty;
                detail.PackageTypes = string.Empty;
                detail.CBM = 0;
                var containerHouses = containers.Where(x => x.container.Hblid == detail.Id);
                if (containerHouses != null)
                {
                    detail.CsMawbcontainers = new List<CsMawbcontainerModel>();
                    foreach (var item in containerHouses)
                    {
                        //detail.ContainerNames = detail.ContainerNames + item.container.Quantity + "x" + item.unit.UnitNameEn + ((item.container.ContainerNo.Length > 0) ? ";" : "");
                        if (item.container.PackageQuantity != null && item.container.PackageTypeId != null)
                        {
                            detail.PackageTypes += item.container.PackageQuantity + "x" + item.PackageTypeName + ", ";

                        }
                        detail.GW = detail.GW + item.container.Gw != null ? item.container.Gw : 0;
                        detail.CBM = detail.CBM + item.container.Cbm != null ? item.container.Cbm : 0;
                        detail.CW = detail.CW + item.container.ChargeAbleWeight != null ? item.container.ChargeAbleWeight : 0;

                        var container = mapper.Map<CsMawbcontainerModel>(item.container);
                        container.CommodityName = item.CommodityName;
                        container.PackageTypeName = item.PackageTypeName;
                        container.ContainerTypeName = item.unit.UnitNameEn;
                        container.UnitOfMeasureName = item.UnitOfMeasureName;
                        detail.CsMawbcontainers.Add(container);
                    }
                }
                //if (detail.ContainerNames.Length > 0 && detail.ContainerNames.ElementAt(detail.ContainerNames.Length - 1) == ';')
                //{
                //    detail.ContainerNames = detail.ContainerNames.Substring(0, detail.ContainerNames.Length - 1);
                //}
                if (detail.PackageTypes.Length > 0 && detail.PackageTypes.ElementAt(detail.PackageTypes.Length - 1) == ';')
                {
                    detail.PackageTypes = detail.PackageTypes.Substring(0, detail.PackageTypes.Length - 1);
                }
            });
            return results;
        }

        public IQueryable<CsTransactionDetailModel> QueryDetail(CsTransactionDetailCriteria criteria)
        {
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            var details = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == criteria.JobId);
            var query = (from detail in details
                         join customer in ((eFMSDataContext)DataContext.DC).CatPartner on detail.CustomerId equals customer.Id into detailCustomers
                         from y in detailCustomers.DefaultIfEmpty()
                         join noti in ((eFMSDataContext)DataContext.DC).CatPartner on detail.NotifyPartyId equals noti.Id into detailNotis
                         from noti in detailNotis.DefaultIfEmpty()
                         join port in ((eFMSDataContext)DataContext.DC).CatPlace on detail.Pod equals port.Id into portDetail
                         from pod in portDetail.DefaultIfEmpty()
                         join fwd in ((eFMSDataContext)DataContext.DC).CatPartner on detail.ForwardingAgentId equals fwd.Id into forwarding
                         from f in forwarding.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).SysUser on detail.SaleManId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         select new { detail, customer = y, notiParty = noti, saleman = x, agent = f, pod });
            if (query == null) return null;
            foreach (var item in query)
            {
                var detail = mapper.Map<CsTransactionDetailModel>(item.detail);
                detail.CustomerName = item.customer?.PartnerNameEn;
                detail.CustomerNameVn = item.customer?.PartnerNameVn;
                detail.SaleManName = item.saleman?.Username;
                detail.NotifyParty = item.notiParty?.PartnerNameEn;
                detail.ForwardingAgentName = item.agent?.PartnerNameEn;
                detail.PODName = item.pod?.NameEn;
                results.Add(detail);
            }
            return results.AsQueryable();
        }

        public IQueryable<CsTransactionDetailModel> QueryById(CsTransactionDetailCriteria criteria)
        {
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            var details = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == criteria.Id);
            var query = (from detail in details
                         join customer in ((eFMSDataContext)DataContext.DC).CatPartner on detail.CustomerId equals customer.Id into detailCustomers
                         from y in detailCustomers.DefaultIfEmpty()
                         join noti in ((eFMSDataContext)DataContext.DC).CatPartner on detail.NotifyPartyId equals noti.Id into detailNotis
                         from noti in detailNotis.DefaultIfEmpty()
                         join port in ((eFMSDataContext)DataContext.DC).CatPlace on detail.Pod equals port.Id into portDetail
                         from pod in portDetail.DefaultIfEmpty()
                         join fwd in ((eFMSDataContext)DataContext.DC).CatPartner on detail.ForwardingAgentId equals fwd.Id into forwarding
                         from f in forwarding.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).CatSaleman on detail.SaleManId equals saleman.Id.ToString() into prods
                         from x in prods.DefaultIfEmpty()
                         select new { detail, customer = y, notiParty = noti, saleman = x, agent = f, pod });
            if (query == null) return null;
            foreach (var item in query)
            {
                var detail = mapper.Map<CsTransactionDetailModel>(item.detail);
                detail.CustomerName = item.customer?.PartnerNameEn;
                detail.CustomerNameVn = item.customer?.PartnerNameVn;
                detail.SaleManId = item.saleman?.Id.ToString();
                detail.NotifyParty = item.notiParty?.PartnerNameEn;
                detail.ForwardingAgentName = item.agent?.PartnerNameEn;
                detail.PODName = item.pod?.NameEn;
                results.Add(detail);
            }
            return results.AsQueryable();
        }

        public CsTransactionDetailModel GetById(CsTransactionDetailCriteria criteria)
        {
            var results = QueryById(criteria).ToList();
            return results.FirstOrDefault();
        }

        #region -- LIST & PAGING HOUSEBILLS --
        public List<CsTransactionDetailModel> Query(CsTransactionDetailCriteria criteria)
        {
            var query = (from detail in DataContext.Get()
                         join tran in csTransactionRepo.Get() on detail.JobId equals tran.Id
                         join customer in catPartnerRepo.Get() on detail.CustomerId equals customer.Id into customers
                         from cus in customers.DefaultIfEmpty()
                         join saleman in catSalemanRepo.Get() on detail.SaleManId equals saleman.Id.ToString() into salemans
                         from sale in salemans.DefaultIfEmpty()
                         select new { detail, tran, cus, sale });
            if (criteria.All == null)
            {
                query = query.Where(x => criteria.JobId != Guid.Empty && criteria.JobId != null ? x.detail.JobId == criteria.JobId : true
                                      && (x.tran.Mawb.IndexOf(criteria.Mawb ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && (x.detail.Hwbno.IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && (x.cus.PartnerNameEn.IndexOf(criteria.CustomerName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && (x.tran.Etd >= criteria.FromDate || criteria.FromDate == null)
                                    && (x.tran.Etd <= criteria.ToDate || criteria.ToDate == null)
                                    && (x.sale.SaleManId.IndexOf(criteria.SaleManName ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    );
            }
            else
            {
                query = query.Where(x => criteria.JobId != Guid.Empty && criteria.JobId != null ? x.detail.JobId == criteria.JobId : true
                                      || (x.tran.Mawb.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0
                                      || (x.detail.Hwbno.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                      || (x.cus.PartnerNameEn.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                      || (x.sale.SaleManId.IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0))
                                      && ((x.tran.Etd ?? null) >= (criteria.FromDate ?? null) && (x.tran.Etd ?? null) <= (criteria.ToDate ?? null))
                                    );
            }
            //List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            //foreach (var item in query)
            //{
            //    var detail = mapper.Map<CsTransactionDetailModel>(item.detail);
            //    detail.CustomerName = item.cus?.PartnerNameEn;
            //    detail.SaleManName = item.sale?.Username;
            //    detail.Etd = item.tran.Etd;
            //    detail.Mawb = item.tran.Mawb;
            //    results.Add(detail);
            //}
            var res = from detail in query.Select(s => s.detail)
                      join tran in csTransactionRepo.Get() on detail.JobId equals tran.Id
                      join customer in catPartnerRepo.Get() on detail.CustomerId equals customer.Id into customers
                      from cus in customers.DefaultIfEmpty()
                      join saleman in sysUserRepo.Get() on detail.SaleManId equals saleman.Id into salemans
                      from sale in salemans.DefaultIfEmpty()
                      join notify in catPartnerRepo.Get() on detail.NotifyPartyId equals notify.Id into notifys
                      from notify in notifys.DefaultIfEmpty()
                      select new CsTransactionDetailModel {
                          Id = detail.Id,
                          JobId = detail.JobId,
                          Hwbno = detail.Hwbno,
                          SaleManId = detail.SaleManId,
                          SaleManName = sale.Username,
                          CustomerId =  detail.CustomerId,
                          CustomerName = cus.ShortName,
                          NotifyPartyId = detail.NotifyPartyId,
                          NotifyParty = notify.ShortName,
                          FinalDestinationPlace = detail.FinalDestinationPlace
                      };
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            results = res.ToList();
            results.ForEach(fe => {
                fe.Containers = string.Join(",", csMawbcontainerRepo.Get(x => x.Hblid == fe.Id)
                                                                        .Select(s => (s.ContainerTypeId != null || s.Quantity != null) ? (s.Quantity + "x" + GetUnitNameById(s.ContainerTypeId)) : string.Empty));
                fe.Packages = string.Join(",", csMawbcontainerRepo.Get(x => x.Hblid == fe.Id)
                                                                        .Select(s => (s.PackageTypeId != null || s.PackageQuantity != null) ? (s.PackageQuantity + "x" + GetUnitNameById(s.PackageTypeId)) : string.Empty));
                fe.GW = csMawbcontainerRepo.Get(x => x.Hblid == fe.Id).Sum(s => s.Gw);
                fe.CBM = csMawbcontainerRepo.Get(x => x.Hblid == fe.Id).Sum(s => s.Cbm);
            });
            return results;
        }

        public List<CsTransactionDetailModel> Paging(CsTransactionDetailCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            rowsCount = data.Count();
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = data.Skip((page - 1) * size).Take(size).ToList();
            }
            
            return results;
        }

        public object GetGoodSummaryOfAllHBLByJobId(Guid JobId)
        {
            var houserbills = DataContext.Get(x => x.JobId == JobId);
            decimal? totalGW = 0;
            decimal? totalNW = 0;
            decimal? totalCW = 0;
            decimal? totalCbm = 0;
            var containers = string.Empty;
            var commodities = string.Empty;

            if (houserbills.Any())
            {
                foreach (var item in houserbills)
                {
                    var containersHbl = csMawbcontainerRepo.Get(x => x.Hblid == item.Id);
                    totalGW += containersHbl.Sum(s => s.Gw);
                    totalNW += containersHbl.Sum(s => s.Nw);
                    totalCW += containersHbl.Sum(s => s.ChargeAbleWeight);
                    totalCbm += containersHbl.Sum(s => s.Cbm);
                    containers += string.Join(",", containersHbl.OrderByDescending(o => o.ContainerTypeId).Select(s => (s.ContainerTypeId != null || s.Quantity != null) ? (s.Quantity + "x" + GetUnitNameById(s.ContainerTypeId)) : string.Empty));
                    commodities += string.Join(",", containersHbl.Select(x => GetCommodityNameById(x.CommodityId)));
                }
            }
            return new { totalGW, totalNW, totalCW, totalCbm , containers , commodities };
        }

        private string GetUnitNameById(short? id)
        {
            var result = string.Empty;
            var data = catUnitRepo.Get(g => g.Id == id).FirstOrDefault();
            result = (data != null) ? data.UnitNameEn : string.Empty;
            return result;
        }

        private string GetCommodityNameById(int? id)
        {
            var result = string.Empty;
            var data = catCommodityRepo.Get(x => x.Id == id).FirstOrDefault();
            result = (data != null) ? data.CommodityNameEn : string.Empty;
            return result;
        }
        #endregion -- LIST & PAGING HOUSEBILLS --

        public object ImportCSTransactionDetail(CsTransactionDetailModel model)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var detail = mapper.Map<CsTransactionDetail>(model);
                detail.Id = Guid.NewGuid();
                //int countDetail = dc.CsTransactionDetail.Count(x => x.JobId == model.JobId);
                //detail.Hwbno = "SEF" + GenerateID.GenerateJobID("HB", countDetail);
                detail.Active = true;
                detail.UserCreated = model.UserCreated;  //ChangeTrackerHelper.currentUser;
                detail.DatetimeCreated = DateTime.Now;
                detail.DesOfGoods = null;
                detail.Commodity = null;
                detail.PackageContainer = null;
                dc.CsTransactionDetail.Add(detail);

                List<CsMawbcontainer> containers = null;
                if (model.CsMawbcontainers.Count > 0)
                {
                    containers = mapper.Map<List<CsMawbcontainer>>(model.CsMawbcontainers);
                }
                else
                {
                    containers = dc.CsMawbcontainer.Where(y => y.Hblid == model.Id).ToList();
                }
                if (containers != null)
                {
                    foreach (var x in containers)
                    {
                        if (x.Id != Guid.Empty)
                        {
                            x.ContainerNo = string.Empty;
                            x.SealNo = string.Empty;
                            x.MarkNo = string.Empty;
                        }
                        x.Id = Guid.NewGuid();
                        x.Hblid = detail.Id;
                        x.Mblid = Guid.Empty;
                        x.UserModified = model.UserCreated;
                        x.DatetimeModified = DateTime.Now;
                        dc.CsMawbcontainer.Add(x);
                    }
                }
                var charges = dc.CsShipmentSurcharge.Where(x => x.Hblid == model.Id);
                if (charges != null)
                {
                    foreach (var charge in charges)
                    {
                        charge.Id = Guid.NewGuid();
                        charge.UserCreated = model.UserCreated;
                        charge.DatetimeCreated = DateTime.Now;
                        charge.Hblid = detail.Id;
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
                dc.SaveChanges();
                var result = new HandleState();
                return new { model = detail, result };
            }
            catch (Exception ex)
            {
                var result = new HandleState(ex.Message);
                return new { model = new object { }, result };
            }
        }

        public HandleState DeleteTransactionDetail(Guid hbId)
        {
            var hs = new HandleState();
            try
            {

                var hbl = DataContext.Where(x => x.Id == hbId).FirstOrDefault();
                if (hbl == null)
                {
                    hs = new HandleState("House Bill not found !");
                }
                else
                {
                    var charges = surchareRepository.Get(x => x.Hblid == hbl.Id).ToList();
                    var isSOA = false;
                    foreach (var item in charges)
                    {
                        if (item.CreditNo != null || item.DebitNo != null || item.Soano != null)
                        {
                            isSOA = true;
                        }
                    }
                    if (isSOA == true)
                    {
                        hs = new HandleState("Cannot delete, this house bill is containing at least one charge have Credit Debit Note/SOA no!");
                    }
                    else
                    {
                        foreach (var item in charges)
                        {
                            surchareRepository.Delete(x => x.Id == item.Id, false);
                        }
                        DataContext.Delete(x => x.Id == hbl.Id);
                        DataContext.SubmitChanges();
                        surchareRepository.SubmitChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }

            return hs;

        }

        public Crystal Preview(CsTransactionDetailModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var parameter = new SeaHBillofLadingITLFrameParameter
            {
                Packages = model.PackageContainer,
                GrossWeight = model.GW == null ? (decimal)model.GW : 0,
                Measurement = string.Empty
            };
            result = new Crystal
            {
                ReportName = "SeaHBillofLadingITLFrame.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            var housebills = new List<SeaHBillofLadingITLFrame>();
            //continue
            var freightCharges = new List<FreightCharge>();
            if (freightCharges.Count == 0)
                return null;
            result.AddDataSource(housebills);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.AddSubReport("Freightcharges", freightCharges);
            result.SetParameter(parameter);
            return result;
        }

    }
}
