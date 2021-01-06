using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPartnerEmailService : IRepositoryBase<CatPartnerEmail, CatPartnerEmailModel>
    {
        IQueryable<CatPartnerEmailModel> GetBy(string partnerId);
        HandleState Delete(Guid id);
        HandleState Update(CatPartnerEmail model);
        HandleState AddEmail(CatPartnerEmailModel model);
        CatPartnerEmailModel GetDetail(Guid id);
    }
}
