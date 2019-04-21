using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Catalogue.Service.ViewModels;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatPlaceService : IRepositoryBase<CatPlace, CatPlaceModel>
    {
        List<vw_catPlace> Query(CatPlaceCriteria criteria);
        List<CatPlaceViewModel> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount);
        List<vw_catProvince> GetProvinces(short? countryId);
        List<vw_catDistrict> GetDistricts(Guid? provinceId);
        List<ModeOfTransport> GetModeOfTransport();
        List<CatPlaceImportModel> CheckValidImport(List<CatPlaceImportModel> list, CatPlaceTypeEnum placeType);
        HandleState Import(List<CatPlaceImportModel> data);
        HandleState Update(CatPlaceModel model);
    }
}
