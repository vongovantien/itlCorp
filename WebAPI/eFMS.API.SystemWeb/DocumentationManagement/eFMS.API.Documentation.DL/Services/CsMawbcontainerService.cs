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
    public class CsMawbcontainerService : RepositoryBase<CsMawbcontainer, CsMawbcontainerModel>, ICsMawbcontainerService
    {
        public CsMawbcontainerService(IContextBase<CsMawbcontainer> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public IQueryable<CsMawbcontainerModel> Query(CsMawbcontainerCriteria criteria)
        {
            var results = Get(x => (x.Mblid == criteria.Mblid || criteria.Mblid == null)
                                && (x.Hblid == criteria.Hblid || criteria.Hblid == null)
                                 );
            return results;
        }
    }
}
