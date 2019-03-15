using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Shipment.Service.Helpers;
using eFMS.Domain.Report;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionDetailService : RepositoryBase<CsTransactionDetail, CsTransactionDetailModel>, ICsTransactionDetailService
    {
        public CsTransactionDetailService(IContextBase<CsTransactionDetail> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState AddTransactionDetail(CsTransactionDetailModel model)
        {
            var detail = mapper.Map<CsTransactionDetail>(model);
            detail.Id = Guid.NewGuid();
            detail.Inactive = false;
            detail.UserCreated = "thor";  //ChangeTrackerHelper.currentUser;
            detail.DatetimeCreated = DateTime.Now;


            try
            {
                DataContext.Add(detail);

                foreach (var x in model.CsMawbcontainers)
                {
                    x.Hblid = detail.Id;
                    x.Id = Guid.NewGuid();
                    //x.Mblid = model.JobId;
                    ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Add(x);

                 }
                 ((eFMSDataContext)DataContext.DC).SaveChanges();
                var hs = new HandleState();
                return hs;

            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public List<CsTransactionDetailModel> GetByJob(CsTransactionDetailCriteria criteria)
        {
            var results = Query(criteria).ToList();
            var containers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer
                .Join(((eFMSDataContext)DataContext.DC).CatUnit,
                    container => container.ContainerTypeId,
                    unit => unit.Id, (container, unit) => new { container, unit })
                    .ToList();
            if (containers.Count() == 0) return results;
            results.ForEach(detail =>
            {
                detail.ContainerNames = string.Empty;
                detail.PackageTypes = string.Empty;
                detail.CBM = 0;
                var containerHouses = containers.Where(x => x.container.Hblid == detail.Id);
                if(containerHouses != null)
                {
                    detail.CsMawbcontainers = new List<CsMawbcontainerModel>();
                    foreach (var item in containerHouses)
                    {
                        detail.ContainerNames = detail.ContainerNames + item.container.Quantity + "x" + item.unit.Code + ((item.container.ContainerNo.Length > 0) ? ";" : "");
                        if (item.container.PackageQuantity != null && item.container.PackageTypeId != null)
                        {
                            detail.PackageTypes = detail.PackageTypes + item.container.PackageQuantity != null ? (item.container.PackageQuantity
                                + item.container.PackageTypeId != null ? ("x" + item.container.PackageTypeId) : string.Empty + item.container.PackageQuantity != null ? "; " : string.Empty) : string.Empty;

                        }
                        detail.CBM = detail.CBM + item.container.Cbm != null ? item.container.Cbm : 0;
                        detail.CsMawbcontainers.DefaultIfEmpty(item.container);
                    }
                }
                if(detail.ContainerNames.Length > 0 && detail.ContainerNames.ElementAt(detail.ContainerNames.Length-1) == ';')
                {
                    detail.ContainerNames = detail.ContainerNames.Substring(0, detail.ContainerNames.Length-1);
                }
                if (detail.PackageTypes.Length > 0 && detail.PackageTypes.ElementAt(detail.PackageTypes.Length - 1) == ';')
                {
                    detail.PackageTypes = detail.PackageTypes.Substring(0, detail.PackageTypes.Length - 1);
                }
            });
            return results;
        }

        public IQueryable<CsTransactionDetailModel> Query(CsTransactionDetailCriteria criteria)
        {
            //var results = Get(x => x.JobId == criteria.JobId);
            List<CsTransactionDetailModel> results = new List<CsTransactionDetailModel>();
            var details = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == criteria.JobId);
            var query = (from detail in details
                         join customer in ((eFMSDataContext)DataContext.DC).CatPartner on detail.CustomerId equals customer.Id into detailCustomers
                         from y in detailCustomers.DefaultIfEmpty()
                         join noti in ((eFMSDataContext)DataContext.DC).CatPartner on detail.NotifyPartyId equals noti.Id into detailNotis
                         from noti in detailNotis.DefaultIfEmpty()
                         join fwd in ((eFMSDataContext)DataContext.DC).CatPartner on  detail.ForwardingAgentId equals fwd.Id into forwarding
                         from f in forwarding.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).SysUser on detail.SaleManId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                         //where detail.JobId == criteria.JobId
                         select new { detail, customer = y, notiParty = noti, saleman = x ,agent = f}
                          );
            if (query == null) return null;
            foreach(var item in query)
            {
                var detail = mapper.Map<CsTransactionDetailModel>(item.detail);
                detail.CustomerName = item.customer?.PartnerNameEn;
                detail.CustomerNameVn = item.customer?.PartnerNameVn;
                detail.SaleManName = item.saleman?.Username;
                detail.NotifyParty = item.notiParty?.PartnerNameEn;
                detail.ForwardingAgentName = item.agent?.PartnerNameEn;
                results.Add(detail);
            }
            return results.AsQueryable();
        }

        public CsTransactionDetailReport GetReportBy(Guid jobId)
        {
            var details = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == jobId);
            var data = (from transDetail in details
                         join customer in ((eFMSDataContext)DataContext.DC).CatPartner on transDetail.CustomerId equals customer.Id into detailCustomers
                         from y in detailCustomers.DefaultIfEmpty()
                         join noti in ((eFMSDataContext)DataContext.DC).CatPartner on transDetail.NotifyPartyId equals noti.Id into detailNotis
                         from noti in detailNotis.DefaultIfEmpty()
                         join fwd in ((eFMSDataContext)DataContext.DC).CatPartner on transDetail.ForwardingAgentId equals fwd.Id into forwarding
                         from f in forwarding.DefaultIfEmpty()
                         join saleman in ((eFMSDataContext)DataContext.DC).SysUser on transDetail.SaleManId equals saleman.Id into prods
                         from x in prods.DefaultIfEmpty()
                             //where detail.JobId == criteria.JobId
                         select new { transDetail, customer = y, notiParty = noti, saleman = x, agent = f }
                          ).FirstOrDefault();
            if (data == null) return null;
            var detail = mapper.Map<CsTransactionDetailReport>(data.transDetail);
            //detail.CustomerName = data.customer?.PartnerNameEn;
            //detail.CustomerNameVn = data.customer?.PartnerNameVn;
            //detail.SaleManName = data.saleman?.Username;
            //detail.NotifyParty = data.notiParty?.PartnerNameEn;
            //detail.ForwardingAgentName = data.agent?.PartnerNameEn;
            return detail;
        }
    }
}
