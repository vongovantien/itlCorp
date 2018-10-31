using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPartnerService : IRepositoryBase<CatPartner, CatPartnerModel>
    {
        IQueryable<CatPartnerModel> Query(CatPartnerCriteria criteria);
        IQueryable<CatPartnerModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount);
    }
}
