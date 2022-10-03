﻿using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
        IQueryable<CatPartnerViewModel2> GetMultiplePartnerGroup(PartnerMultiCriteria criteria);
        HandleState CheckDetailPermission(string id);
        CatPartnerModel GetDetail(string id);
        HandleState CheckDeletePermission(string id);
        bool SendMailRejectComment(string partnerId, string comment);
        bool SendMailCreatedSuccess(CatPartner catPartner);
        List<CatPartnerViewModel> GetSubListPartnerByID(string id);
        HandleState UpdatePartnerData(CatPartnerModel model);
        IQueryable<QueryExportAgreementInfo> QueryExportAgreement(CatPartnerCriteria criteria);
        List<SysUserViewModel> GetListSaleman(string partnerId, string transactionType, string shipmentType);
        IQueryable<CatPartnerForKeyinCharge> GetPartnerForKeyinCharge(PartnerMultiCriteria criteria);
        Task<CatPartnerModel> GetPartnerByTaxCode (string taxCode);

    }
}
