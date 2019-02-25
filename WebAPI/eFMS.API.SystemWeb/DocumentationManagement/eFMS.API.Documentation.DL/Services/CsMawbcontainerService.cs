using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsMawbcontainerService : RepositoryBase<CsMawbcontainer, CsMawbcontainerModel>, ICsMawbcontainerService
    {
        public CsMawbcontainerService(IContextBase<CsMawbcontainer> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<vw_csMAWBContainer> Query(CsMawbcontainerCriteria criteria)
        {
            var data = GetView();
            var results = data.Where(x => (x.MBLID == criteria.Mblid || criteria.Mblid == null)
                                && (x.HBLID == criteria.Hblid || criteria.Hblid == null)
                                 ).ToList();
            
            return results;
        }

        private List<vw_csMAWBContainer> GetView(){
            
            List<vw_csMAWBContainer> results = ((eFMSDataContext)DataContext.DC).GetViewData<vw_csMAWBContainer>();
            return results;
        }
    }
}
