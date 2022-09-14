﻿using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatContractService : IRepositoryBaseCache<CatContract, CatContractModel>
    {
        CatContract GetContractById(Guid Id);
        IQueryable<CatContract> GetContracts();
        IQueryable<CatContractViewModel> Query(CatContractCriteria criteria);
        List<CatContractViewModel> Paging(CatContractCriteria criteria, int page, int size, out int rowsCount);
        HandleState Delete(Guid id);
        HandleState Update(CatContractModel model, out bool isChangeAgrmentType);
        HandleState CustomerRequest(CatContractModel model);
        List<CatContractModel> GetBy(string partnerId, bool? all);
        object GetContractIdByPartnerId(string partnerId, string jobId);
        //Task<ResultHandle> UploadContractFile(ContractFileUploadModel model);
        //Task<ResultHandle> UploadMoreContractFile(UploadFileMoreContractModel model);
        Task<ResultHandle> UploadMoreContractFile(List<ContractFileUploadModel> model);
        CatContractModel GetById(Guid Id);
        //SysImage GetFileContract(string partnerId, string contractId);
        HandleState UpdateFileToContract(List<SysImage> files);
        Task<HandleState> DeleteFileContract(Guid id);
        HandleState ActiveInActiveContract(Guid id, string partnerId, SalesmanCreditModel credit, out bool active);
        IQueryable<CatContract> CheckExistedContractActive(Guid id, string partnerId);
        CatContract CheckExistedContractInActive(Guid id, string partnerId, out List<ServiceOfficeGroup> serviceOfficeGrps);
        List<CatContractImportModel> CheckValidImport(List<CatContractImportModel> list);
        HandleState Import(List<CatContractImportModel> data);
        bool SendMailRejectComment(string partnerId, string contractId, string comment, string partnerType);
        bool SendMailARConfirmed(string partnerId, string contractId, string partnerType);
        IQueryable<CatAgreementModel> QueryAgreement(CatContractCriteria criteria);
        string CheckExistedContract(CatContractModel model);
    }
}
