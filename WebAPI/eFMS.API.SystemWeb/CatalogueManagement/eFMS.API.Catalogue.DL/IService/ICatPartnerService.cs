using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPartnerService : IRepositoryBase<CatPartner, CatPartnerModel>
    {
        List<CatPartnerViewModel> Query(CatPartnerCriteria criteria);
        IQueryable<CatPartnerViewModel> Paging(CatPartnerCriteria criteria, int page, int size, out int rowsCount);
        List<CustomerPartnerViewModel> PagingCustomer(CatPartnerCriteria criteria, int page, int size, out int rowsCount);
        List<DepartmentPartner> GetDepartments();
        List<CatPartnerImportModel> CheckValidImport(List<CatPartnerImportModel> list);
        HandleState Import(List<CatPartnerImportModel> data);
        HandleState Delete(string id);
        HandleState Update(CatPartnerModel model);
    }
}
