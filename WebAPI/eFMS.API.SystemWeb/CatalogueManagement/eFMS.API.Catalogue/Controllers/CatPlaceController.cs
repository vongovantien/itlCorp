using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Helpers;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;
using System.Linq;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPlaceController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPlaceService catPlaceService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;
        private string templateName = "ImportTemplate.xlsx";

        public CatPlaceController(IStringLocalizer<LanguageSub> localizer, ICatPlaceService service, IMapper iMapper, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catPlaceService = service;
            mapper = iMapper;
            currentUser = user;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = catPlaceService.Get();
            return Ok(results);
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatPlaceCriteria criteria)
        {
            var results = catPlaceService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatPlaceCriteria criteria, int page, int size)
        {
            var data = catPlaceService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var data = catPlaceService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetProvinces")]
        public IActionResult GetProvinces(short? countryId)
        {
            var results = catPlaceService.GetProvinces(countryId);
            return Ok(results);
        }

        [HttpGet]
        [Route("GetDistricts")]
        public IActionResult GetDistricts(Guid? provinceId)
        {
            var results = catPlaceService.GetDistricts(provinceId);
            return Ok(results);
        }

        [HttpGet]
        [Route("GetModeOfTransport")]
        public IActionResult GetModeOfTransport()
        {
            return Ok(catPlaceService.GetModeOfTransport());
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatPlaceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(Guid.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.PlaceTypeId = PlaceTypeEx.GetPlaceType(model.PlaceType);
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.Id = Guid.NewGuid();
            catPlace.UserCreated = currentUser.UserID;
            catPlace.DatetimeCreated = DateTime.Now;
            catPlace.Inactive = false;
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catPlaceService.Add(catPlace);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(Guid id, CatPlaceEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var catPlace = mapper.Map<CatPlaceModel>(model);
            catPlace.UserModified = currentUser.UserID;
            catPlace.DatetimeModified = DateTime.Now;
            catPlace.Id = id;
            if(catPlace.Inactive == true)
            {
                catPlace.InactiveOn = DateTime.Now;
            }
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = catPlaceService.Update(catPlace, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = catPlaceService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel(CatPlaceTypeEnum type)
        {
            templateName = GetFileName(type);
            var result = await new FileHelper().ExportExcel(templateName);
            if (result != null)
                return result;
            else return BadRequest(result);
        }

        [HttpPost]
        [Route("UpLoadFile")]
        [Authorize]
        public IActionResult UpLoadFile(IFormFile uploadedFile, CatPlaceTypeEnum type)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if(file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest();
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
            return BadRequest(file);
        }

        private List<CatPlaceImportModel> ReadWarehouseFromExel(ExcelWorksheet worksheet, int rowCount)
        {
            List<CatPlaceImportModel> list = new List<CatPlaceImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                var warehouse = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString(),
                    Address = worksheet.Cells[row, 4].Value?.ToString(),
                    CountryName = worksheet.Cells[row, 5].Value?.ToString(),
                    ProvinceName = worksheet.Cells[row, 6].Value?.ToString(),
                    DistrictName = worksheet.Cells[row, 7].Value?.ToString(),
                    Status = worksheet.Cells[row, 8].Value?.ToString()
                };
                list.Add(warehouse);
            }
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
                    Code = worksheet.Cells[row, 1].Value?.ToString(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString(),
                    Address = worksheet.Cells[row, 4].Value?.ToString(),
                    CountryName = worksheet.Cells[row, 5].Value?.ToString(),
                    ProvinceName = worksheet.Cells[row, 6].Value?.ToString(),
                    DistrictName = worksheet.Cells[row, 7].Value?.ToString(),
                    Status = worksheet.Cells[row, 8].Value?.ToString()
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
                    Code = worksheet.Cells[row, 1].Value?.ToString(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString(),
                    Status = worksheet.Cells[row, 5].Value?.ToString()
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
                var warehouse = new CatPlaceImportModel
                {
                    IsValid = true,
                    Code = worksheet.Cells[row, 1].Value?.ToString(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString(),
                    ProvinceName = worksheet.Cells[row, 5].Value?.ToString(),
                    Status = worksheet.Cells[row, 6].Value?.ToString()
                };
                list.Add(warehouse);
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
                    Code = worksheet.Cells[row, 1].Value?.ToString(),
                    NameEn = worksheet.Cells[row, 2].Value?.ToString(),
                    NameVn = worksheet.Cells[row, 3].Value?.ToString(),
                    CountryName = worksheet.Cells[row, 4].Value?.ToString(),
                    ProvinceName = worksheet.Cells[row, 5].Value?.ToString(),
                    DistrictName = worksheet.Cells[row, 6].Value?.ToString(),
                    Status = worksheet.Cells[row, 7].Value?.ToString()
                };
                list.Add(warehouse);
            }
            return list;
        }

        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody]List<CatPlaceImportModel> data)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var result = catPlaceService.Import(data);
            return Ok(result);
        }

        private string GetFileName(CatPlaceTypeEnum type)
        {
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
                if (catPlaceService.Any(x => (x.Code.ToLower() == model.Code.ToLower()) || (x.NameEn.ToLower()== model.NameEN.ToLower()) || (x.NameVn.ToLower()==model.NameVN.ToLower()) ))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catPlaceService.Any(x => ((x.Code.ToLower() == model.Code.ToLower()) || (x.NameEn.ToLower() == model.NameEN.ToLower()) || (x.NameVn.ToLower() == model.NameVN.ToLower())) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}