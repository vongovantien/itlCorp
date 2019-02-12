using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsTransactionDetailService : RepositoryBase<CsTransactionDetail, CsTransactionDetailModel>, ICsTransactionDetailService
    {
        public CsTransactionDetailService(IContextBase<CsTransactionDetail> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<CsTransactionDetailModel> GetByJob(CsTransactionDetailCriteria criteria)
        {
            var results = Query(criteria).ToList();
            var containers = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Mblid == criteria.JobId).ToList();
            if (containers.Count() == 0) return results;
            results.ForEach(detail =>
            {
                detail.ContainerNames = string.Empty;
                detail.PackageTypes = string.Empty;
                detail.CBM = 0;
                var containerHouses = containers.Where(x => x.Hblid == detail.Id);
                foreach(var item in containerHouses)
                {
                    detail.ContainerNames = detail.ContainerNames + item.ContainerNo + "; ";
                    detail.PackageTypes = detail.PackageTypes + item.PackageQuantity!= null?(item.PackageQuantity + item.PackageTypeId!= null?("x" + item.PackageTypeId): string.Empty + "; "): string.Empty;
                    detail.CBM = detail.CBM + item.Cbm != null? item.Cbm: 0;
                }
            });
            return results;
        }

        public IQueryable<CsTransactionDetailModel> Query(CsTransactionDetailCriteria criteria)
        {
            var results = Get(x => x.JobId == criteria.JobId);
            return results;
        }
    }
}
