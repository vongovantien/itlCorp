using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System.Linq;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPlaceController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPlaceService catPlaceService;
        private readonly IMapper mapper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICurrentUser currentUser;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatPlaceService</param>
        /// <param name="iMapper">inject interface IMapper</param>
        /// <param name="curUser"></param>
        /// <param name="hostingEnvironment"></param>
        public CatPlaceController(IStringLocalizer<LanguageSub> localizer,
            ICatPlaceService service,
            IMapper iMapper,
            ICurrentUser curUser,
            IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catPlaceService = service;
            mapper = iMapper;
            currentUser = curUser;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get all places
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = catPlaceService.Get();
            return Ok(results);
        }
        /// <summary>
        /// get by mode tran
        /// </summary>
        /// <returns></returns>
        [Route("GetByModeTran")]
        [HttpGet]
        public IActionResult GetByModeTran()
        {
            var results = catPlaceService.GetByModeOfTran();
            return Ok(results);
        }

        /// <summary>
        /// get the list of all places
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatPlaceCriteria criteria)
        {
            var results = catPlaceService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("QueryExport")]
        public IActionResult QueryExport(CatPlaceCriteria criteria)
        {
            var results = catPlaceService.QueryExport(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of places by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]

        public IActionResult Get(CatPlaceCriteria criteria, int page, int size)
        {
            var data = catPlaceService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;
            CatPlaceModel place = catPlaceService.Get(x => x.Id == id).FirstOrDefault();
            if (place == null)
            {
                return Ok(false);
            }
            if (place.PlaceTypeId == CatPlaceTypeEnum.Warehouse.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
            }
            if (place.PlaceTypeId == CatPlaceTypeEnum.Port.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
            }

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            return Ok(catPlaceService.CheckAllowPermissionAction(id, permissionRange));
        }

        /// <summary>
        /// get place by id
        /// </summary>
        /// <param name="id">id to retrieve data</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var data = catPlaceService.GetDetail(id);
            return Ok(data);
        }

        /// <summary>
        /// get the list of provinces by country
        /// </summary>
        /// <param name="countryId">country id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProvinces")]
        public IActionResult GetProvinces(short? countryId)
        {
            var results = catPlaceService.GetProvinces(countryId);
            return Ok(results);
        }

        /// <summary>
        /// get all province
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllProvinces")]
        public IActionResult GetAllProvinces()
        {
            var results = catPlaceService.GetAllProvinces();
            return Ok(results);
        }


        /// <summary>
        /// get the list of districts by province
        /// </summary>
        /// <param name="provinceId">province id</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDistricts")]
        public IActionResult GetDistricts(Guid? provinceId)
        {
            var results = catPlaceService.GetDistricts(provinceId);
            return Ok(results);
        }


        /// <summary>
        /// get the list of mode
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetModeOfTransport")]
        public IActionResult GetModeOfTransport()
        {
            return Ok(catPlaceService.GetModeOfTransport());
        }

        /// <summary>
        /// add new place
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatPlaceEditModel model)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;
            if (model.PlaceType == CatPlaceTypeEnum.Warehouse)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
            }
            if (model.PlaceType == CatPlaceTypeEnum.Port)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
            }
            if (model.PlaceType == CatPlaceTypeEnum.Province || model.PlaceType == CatPlaceTypeEnum.District || model.PlaceType == CatPlaceTypeEnum.Ward)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catLocation);
            }

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });

            }

            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            model.PlaceTypeId = PlaceTypeEx.GetPlaceType(model.PlaceType);
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.GroupId = currentUser.GroupId;
            catPlace.DepartmentId = currentUser.DepartmentId;
            catPlace.OfficeId = currentUser.OfficeID;
            catPlace.CompanyId = currentUser.CompanyID;

            var hs = catPlaceService.Add(catPlace);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="id">id of data that want to update</param>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(Guid id, CatPlaceEditModel model)
        {

            PermissionRange permissionRange;
            ICurrentUser _user = null;
            if (model.PlaceType == CatPlaceTypeEnum.Warehouse)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
            }
            if (model.PlaceType == CatPlaceTypeEnum.Port)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
            }
            if (model.PlaceType == CatPlaceTypeEnum.Province || model.PlaceType == CatPlaceTypeEnum.District || model.PlaceType == CatPlaceTypeEnum.Ward)
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catLocation);
            }


            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);
            if (permissionRange == PermissionRange.None)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }
            bool code = catPlaceService.CheckAllowPermissionAction(id, permissionRange);

            if (code == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });

            }
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.Id = id;
            var hs = catPlaceService.Update(catPlace);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// check permission delete item
        /// </summary>
        /// <param name="id">id of data that want to delete</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            PermissionRange permissionRange;
            ICurrentUser _user = null;
            CatPlaceModel place = catPlaceService.Get(x => x.Id == id).FirstOrDefault();
            if (place == null)
            {
                return Ok(false);
            }
            if (place.PlaceTypeId == CatPlaceTypeEnum.Warehouse.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
            }
            if (place.PlaceTypeId == CatPlaceTypeEnum.Port.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
            }
            if (place.PlaceTypeId == CatPlaceTypeEnum.Province.ToString() || place.PlaceTypeId == CatPlaceTypeEnum.District.ToString() || place.PlaceTypeId == CatPlaceTypeEnum.Ward.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catLocation);
            }

            permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);
            return Ok(catPlaceService.CheckAllowPermissionAction(id, permissionRange));
        }
        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catPlaceService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// download an excel file from server
        /// </summary>
        /// <param name="type">type of partner</param>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel(CatPlaceTypeEnum type)
        {
            string fileName = GetFileName(type);
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// read data from excel file
        /// </summary>
        /// <param name="uploadedFile">file upload</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpLoadFile")]
        //[Authorize]
        public IActionResult UpLoadFile(IFormFile uploadedFile, CatPlaceTypeEnum type)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });

                List<CatPlaceImportModel> list = null;
                switch (type)
                {
                    case CatPlaceTypeEnum.Warehouse:
                        list = ReadWarehouseFromExel(worksheet, rowCount);
                        break;
                    case CatPlaceTypeEnum.Port:
                        list = ReadPortIndexFromExel(worksheet, rowCount);
                        break;
                    case CatPlaceTypeEnum.Province:
                        list = ReadProvinceFromExel(worksheet, rowCount);
                        break;
                    case CatPlaceTypeEnum.District:
                        list = ReadDistrictFromExel(worksheet, rowCount);
                        break;
                    case CatPlaceTypeEnum.Ward:
                        list = ReadWardFromExel(worksheet, rowCount);
                        break;
                }

                var data = catPlaceService.CheckValidImport(list, type);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        private List<CatPlaceImportModel> ReadWarehouseFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<CatPlaceImportModel> list = new List<CatPlaceImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var warehouse = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                    Address = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                    CountryName = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                    ProvinceName = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                    DistrictName = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                    Status = "Active"
                };
                list.Add(warehouse);
            }
            list = list.Where(x => x.Code != null
                || x.NameEn != null
                || x.NameVn != null
                || x.Address != null
                || x.CountryName != null
                || x.ProvinceName != null
                || x.DisplayName != null).ToList();
            return list;
        }
        private List<CatPlaceImportModel> ReadPortIndexFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<CatPlaceImportModel> list = new List<CatPlaceImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var warehouse = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                    AreaName = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                    ModeOfTransport = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                    Status = worksheet.Cells[row, 7].Value?.ToString().Trim()
                };
                list.Add(warehouse);
            }
            return list;
        }
        private List<CatPlaceImportModel> ReadProvinceFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<CatPlaceImportModel> list = new List<CatPlaceImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var warehouse = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                    Status = worksheet.Cells[row, 5].Value?.ToString().Trim()
                };
                list.Add(warehouse);
            }
            return list;
        }
        private List<CatPlaceImportModel> ReadDistrictFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<CatPlaceImportModel> list = new List<CatPlaceImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var district = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                    ProvinceName = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                    Status = worksheet.Cells[row, 6].Value?.ToString().Trim()
                };
                list.Add(district);
            }
            return list;
        }

        private List<CatPlaceImportModel> ReadWardFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<CatPlaceImportModel> list = new List<CatPlaceImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var warehouse = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                    ProvinceName = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                    DistrictName = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                    Status = worksheet.Cells[row, 7].Value?.ToString().Trim()
                };
                list.Add(warehouse);
            }
            return list;
        }

        /// <summary>
        /// import partner data
        /// </summary>
        /// <param name="data">data to import</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody]List<CatPlaceImportModel> data)
        {
            var hs = catPlaceService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string GetFileName(CatPlaceTypeEnum type)
        {
            string templateName = Templates.ExelImportEx;
            switch (type)
            {
                case CatPlaceTypeEnum.Port:
                    templateName = "PortIndex" + templateName;
                    break;
                case CatPlaceTypeEnum.Province:
                    templateName = "Province" + templateName;
                    break;
                case CatPlaceTypeEnum.District:
                    templateName = "District" + templateName;
                    break;
                case CatPlaceTypeEnum.Ward:
                    templateName = "Ward" + templateName;
                    break;
                default:
                    templateName = "Warehouse" + templateName;
                    break;
            }
            return templateName;
        }

        private string CheckExist(Guid id, CatPlaceEditModel model)
        {
            string message = string.Empty;
            if (id == Guid.Empty)
            {
                if (catPlaceService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.PlaceTypeId == PlaceTypeEx.GetPlaceType(model.PlaceType)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }

            }
            else
            {
                if (catPlaceService.Any(x => x.Code.ToLower() == model.Code.ToLower() && x.Id != id && x.PlaceTypeId == PlaceTypeEx.GetPlaceType(model.PlaceType)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }



    }
}