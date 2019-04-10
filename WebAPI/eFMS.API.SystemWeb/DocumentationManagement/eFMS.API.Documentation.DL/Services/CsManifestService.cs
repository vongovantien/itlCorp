using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsManifestService : RepositoryBase<CsManifest, CsManifestModel>, ICsManifestService
    {
        public CsManifestService(IContextBase<CsManifest> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState AddOrUpdateManifest(CsManifestEditModel model)
        {
            try
            {
                var manifest = mapper.Map<CsManifest>(model);
                manifest.CreatedDate = DateTime.Now;
                var hs = new HandleState();
                if (DataContext.Any(x => x.JobId == model.JobId))
                {
                    hs = DataContext.Update(manifest, x => x.JobId == model.JobId);
                }
                else
                {
                    hs = DataContext.Add(manifest);
                }
                if (hs.Success)
                {
                    foreach(var item in model.CsTransactionDetails)
                    {
                        if (item.IsRemoved)
                        {
                            item.ManifestRefNo = null;
                        }
                        else
                        {
                            item.ManifestRefNo = manifest.RefNo;
                        }
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = manifest.UserCreated;
                        ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Update(item);
                    }
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }

                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public CsManifestModel GetById(Guid jobId)
        {
            var query = (from manifest in ((eFMSDataContext)DataContext.DC).CsManifest
                         where manifest.JobId == jobId
                         join pol in ((eFMSDataContext)DataContext.DC).CatPlace on manifest.Pol equals pol.Id into polManifest
                         from pl in polManifest.DefaultIfEmpty()
                         join pod in ((eFMSDataContext)DataContext.DC).CatPlace on manifest.Pod equals pod.Id into podManifest
                         from pd in polManifest.DefaultIfEmpty()
                         select new { manifest, pl, pd }).FirstOrDefault();
            if (query == null) return null;
            var result = mapper.Map<CsManifestModel>(query.manifest);
            result.PodName = query.pd?.NameEn;
            result.PolName = query.pl?.NameEn;
            return result;
        }

        public Crystal Preview(ManifestReportModel model)
        {
            Crystal result = new Crystal();
            var housebillList = new List<HouseBillManifestModel>();
            if (model.CsMawbcontainers != null)
            {
                string sbContNo = "";
                string sbContType = "";
                foreach (var container in model.CsMawbcontainers)
                {
                    if (!string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                    {
                        sbContNo += container.ContainerNo + "/ " + container.SealNo + ", ";
                    }
                    else if (!string.IsNullOrEmpty(container.ContainerNo) && string.IsNullOrEmpty(container.SealNo))
                    {
                        sbContNo += container.ContainerNo + ", ";
                    }
                    else if (string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                    {
                        sbContNo += container.SealNo + ", ";
                    }
                    sbContType += container.Quantity + "X" + container.ContainerTypeName + ", ";
                }
                if (sbContNo.Length > 2)
                {
                    model.SealNoContainerNames = sbContNo.Substring(0, sbContNo.Length - 2);
                }
                if (sbContType.Length > 2)
                {
                    model.NumberContainerTypes = sbContType.Substring(0, sbContType.Length - 2);
                }
            }
            var manifest = new ManifestReportResult
            {
                POD = model.PodName,
                POL = model.PolName,
                VesselNo = model.VoyNo,
                ETD = model.InvoiceDate,
                SealNoContainerNames = model.SealNoContainerNames,
                NumberContainerTypes = model.NumberContainerTypes,
            };
            decimal sumGW = 0;
            decimal sumVolumn = 0;
            if (model.CsTransactionDetails != null)
            {
                foreach (var item in model.CsTransactionDetails)
                {
                    var houseBill = new HouseBillManifestModel
                    {
                        Hwbno = item.Hwbno,
                        Packages = item.PackageContainer,
                        Weight = (decimal)item.GW,
                        Volumn = (decimal)item.CBM,
                        Shipper = item.ShipperDescription,
                        NotifyParty = item.NotifyPartyDescription,
                        ShippingMark = item.ShippingMark,
                        Description = item.DesOfGoods,
                        FreightPayment = item.FreightPayment
                    };
                    sumGW += (decimal)item?.GW;
                    sumVolumn += (decimal)item?.CBM;
                    housebillList.Add(houseBill);
                }
            }
            if(housebillList.Count > 0)
            {
                housebillList.ForEach(x => {
                    x.SumGrossWeight = sumGW;
                    x.SumVolumn = sumVolumn;
                });
            }
            result.AddDataSource(new List<ManifestReportResult> { manifest });
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.AddSubReport("ManifestHouseBillDetail", housebillList);
            return result;
        }
    }
}
