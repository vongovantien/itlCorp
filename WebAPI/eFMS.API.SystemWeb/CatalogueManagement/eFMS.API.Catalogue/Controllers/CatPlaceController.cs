using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System.Linq;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using ITL.NetCore.Connection.NoSql;

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
            catPlace.UserCreated = currentUser.UserID;
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
            catPlace.Id = id;
            //var hs = catPlaceService.Update(catPlace, x => x.Id == id);
            var hs = catPlaceService.Update(catPlace);
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
            var hs = catPlaceService.Delete(id);
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
            string templateName = GetFileName(type);
            var result = await new FileHelper().ExportExcel(templateName);
            if (result != null)
            {
                return result;
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        [HttpPost]
        [Route("UpLoadFile")]
        //[Authorize]
        public IActionResult UpLoadFile(IFormFile uploadedFile, CatPlaceTypeEnum type)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if(file != null)
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

        [HttpPost]
        [Route("Import")]
        [Authorize]
        public IActionResult Import([FromBody]List<CatPlaceImportModel> data)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var result = catPlaceService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
            }
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
                if (catPlaceService.Any(x => x.Code.ToLower() == model.Code.ToLower() && (x.NameEn.ToLower() == model.NameEn.ToLower() || x.NameVn.ToLower() == model.NameVn.ToLower())))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (catPlaceService.Any(x => x.Code.ToLower() == model.Code.ToLower() && (x.NameEn.ToLower() == model.NameEn.ToLower() || x.NameVn.ToLower() == model.NameVn.ToLower()) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}