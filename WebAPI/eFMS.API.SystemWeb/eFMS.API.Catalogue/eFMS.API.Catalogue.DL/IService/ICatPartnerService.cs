using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPartnerService : IRepositoryBaseCache<CatPartner, CatPartnerModel>
    {
        IQueryable<CatPartnerModel> GetPartners();
        IQueryable<CatPartnerModel> GetBy(CatPartnerGroupEnum partnerGroup);
        IQueryable<CatPartnerViewModel> Query(CatPartnerCriteria criteria);
        IQueryable<CatPartnerViewModel> QueryExport(CatPartnerCriteria criteria);
        IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount);
        List<DepartmentPartner> GetDepartments();
        List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list);
        List<CatPartnerImportModel> CheckValidCustomerAgentImport(List<CatPartnerImportModel> list);

        HandleState Import(List<CatPartnerImportModel> data);
        HandleState ImportCustomerAgent(List<CatPartnerImportModel> data,string type);

        HandleState Delete(string id);
        HandleState Update(CatPartnerModel model);
        object Add(CatPartnerModel model);

        IQueryable<CatPartnerViewModel> GetMultiplePartnerGroup(PartnerMultiCriteria criteria);
        int CheckDetailPermission(string id);
        CatPartnerModel GetDetail(string id);
        HandleState CheckDeletePermission(string id);
        bool SendMailRejectComment(string partnerId, string comment);
        List<CatPartnerViewModel> GetSubListPartnerByID(string id, string partnerType);

    }
}
