using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Operation.DL.IService
{
    public interface ICustomsDeclarationService : IRepositoryBase<CustomsDeclaration, CustomsDeclarationModel>
    {
        IQueryable<CustomsDeclarationModel> GetAll();
        object GetClearanceTypeData();
        HandleState ImportClearancesFromEcus();
        List<CustomsDeclarationModel> Paging(CustomsDeclarationCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CustomsDeclarationModel> GetCustomDeclaration(string keysearch, string customerNo, bool impPorted, int pageNumber, int pageSize, out int rowsCount);
        IQueryable<CustomsDeclarationModel> Query(CustomsDeclarationCriteria criteria);
        List<CustomsDeclarationModel> GetBy(string jobNo);
        HandleState UpdateJobToClearances(List<CustomsDeclarationModel> clearances);
        CustomsDeclaration GetById(int id);
        HandleState CheckAllowDelete(List<CustomsDeclarationModel> customs);
        HandleState DeleteMultiple(List<CustomsDeclarationModel> customs);
        List<CustomClearanceImportModel> CheckValidImport(List<CustomClearanceImportModel> list);
        HandleState Import(List<CustomsDeclarationModel> data);
        List<CustomsDeclarationModel> GetCustomsShipmentNotLocked();
        int CheckDetailPermission(int id);
        CustomsDeclarationModel GetDetail(int id);
        List<CustomsDeclarationModel> GetListCustomNoAsignPIC();
        bool CheckAllowUpdate(int id);
    }
}
