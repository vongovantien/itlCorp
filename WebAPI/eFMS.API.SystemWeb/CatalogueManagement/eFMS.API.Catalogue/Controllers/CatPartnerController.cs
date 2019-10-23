using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPartnerController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerService catPartnerService;
        private readonly IMapper mapper;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatPartnerService</param>
        /// <param name="iMapper">inject interface IMapper</param>
        public CatPartnerController(IStringLocalizer<LanguageSub> localizer, ICatPartnerService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            catPartnerService = service;
            mapper = iMapper;
        }

        /// <summary>
        /// get all partners
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var results = catPartnerService.GetPartners();
            return Ok(results);
        }

        /// <summary>
        /// get the list of partners
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatPartnerCriteria criteria)
        {
            var results = catPartnerService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get partners by partner group
        /// </summary>
        /// <param name="partnerGroup"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(CatPartnerGroupEnum partnerGroup)
        {
            var results = catPartnerService.GetBy(partnerGroup);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of commodities by partners
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatPartnerCriteria criteria, int page, int size)
        {
            var data = catPartnerService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// et and paging the list of partners by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("PagingCustomer")]
        public IActionResult GetCustomer(CatPartnerCriteria criteria, int page, int size)
        {
            var data = catPartnerService.PagingCustomer(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get partner by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var data = catPartnerService.First(x => x.Id == id);
            return Ok(data);
        }

        [HttpGet("CheckTaxCode")]
        public IActionResult CheckTaxCode(string taxcode)
        {
            var result = catPartnerService.Get(x => x.TaxCode.Trim() == taxcode.Trim()).FirstOrDefault();
            return Ok(result);
        }


        /// <summary>
        /// add new partner
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatPartnerEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var partner = mapper.Map<CatPartnerModel>(model);
            var hs = catPartnerService.Add(partner);
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
        /// <param name="id">id of data that need to update</param>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        public IActionResult Put(string id, CatPartnerEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var partner = mapper.Map<CatPartnerModel>(model);
            partner.Id = id;
            var hs = catPartnerService.Update(partner);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            var hs = catPartnerService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get department
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDepartments")]
        public IActionResult GetDepartments()
        {
            return Ok(catPartnerService.GetDepartments());
        }
        private string CheckExist(string id, CatPartnerEditModel model)
        {
            string message = string.Empty;
            if (id.Length == 0)
            {
                if (catPartnerService.Any(x => x.PartnerNameEn == model.PartnerNameEn || x.PartnerNameVn == model.PartnerNameVn || x.ShortName == model.ShortName))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catPartnerService.Any(x => (x.PartnerNameEn == model.PartnerNameEn || x.PartnerNameVn == model.PartnerNameVn) && x.Id != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }

        /// <summary>
        /// import list partner
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatPartnerImportModel> data)
        {
            var result = catPartnerService.Import(data);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(new ResultHandle { Status = false, Message = result.Exception.Message });
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string templateName = Templates.CatPartner.ExelImportFileName + Templates.ExelImportEx;
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

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CatPartnerImportModel> list = new List<CatPartnerImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    var stage = new CatPartnerImportModel
                    {
                        IsValid = true,
                        ShortName = worksheet.Cells[row, 1].Value == null ? string.Empty : worksheet.Cells[row, 1].Value.ToString(),
                        PartnerNameEn = worksheet.Cells[row, 2].Value == null ? string.Empty : worksheet.Cells[row, 2].Value.ToString(),
                        PartnerNameVn = worksheet.Cells[row, 3].Value == null ? string.Empty : worksheet.Cells[row, 3].Value.ToString(),
                        TaxCode = worksheet.Cells[row, 4].Value == null ? string.Empty : worksheet.Cells[row, 4].Value.ToString(),
                        PartnerGroup = worksheet.Cells[row, 5].Value == null ? string.Empty : worksheet.Cells[row, 5].Value.ToString(),
                        ContactPerson = worksheet.Cells[row, 6].Value == null ? string.Empty : worksheet.Cells[row, 6].Value.ToString(),
                        Tel = worksheet.Cells[row, 7].Value == null ? string.Empty : worksheet.Cells[row, 7].Value.ToString(),
                        AddressEn = worksheet.Cells[row, 8].Value == null ? string.Empty : worksheet.Cells[row, 8].Value.ToString(),
                        AddressVn = worksheet.Cells[row, 9].Value == null ? string.Empty : worksheet.Cells[row, 9].Value.ToString(),
                        CityBilling = worksheet.Cells[row, 10].Value == null ? string.Empty : worksheet.Cells[row, 10].Value.ToString(),
                        CountryBilling = worksheet.Cells[row, 11].Value == null ? string.Empty : worksheet.Cells[row, 11].Value.ToString(),
                        ZipCode = worksheet.Cells[row, 12].Value == null ? string.Empty : worksheet.Cells[row, 12].Value.ToString(),
                        AddressShippingEn = worksheet.Cells[row, 13].Value == null ? string.Empty : worksheet.Cells[row, 13].Value.ToString(),
                        AddressShippingVn = worksheet.Cells[row, 14].Value == null ? string.Empty : worksheet.Cells[row, 14].Value.ToString(),
                        CityShipping = worksheet.Cells[row, 15].Value == null ? string.Empty : worksheet.Cells[row, 15].Value.ToString(),
                        CountryShipping = worksheet.Cells[row, 16].Value == null ? string.Empty : worksheet.Cells[row, 16].Value.ToString(),
                        ZipCodeShipping = worksheet.Cells[row, 17].Value == null ? string.Empty : worksheet.Cells[row, 17].Value.ToString(),
                        Email = worksheet.Cells[row, 18].Value == null ? string.Empty : worksheet.Cells[row, 18].Value.ToString(),
                        SaleManName = worksheet.Cells[row, 19].Value == null ? string.Empty : worksheet.Cells[row, 19].Value.ToString(),
                        Profile = worksheet.Cells[row, 20].Value == null ? string.Empty : worksheet.Cells[row, 20].Value.ToString(),
                        BankAccountNo = worksheet.Cells[row, 21].Value == null ? string.Empty : worksheet.Cells[row, 21].Value.ToString(),
                        BankAccountName = worksheet.Cells[row, 22].Value == null ? string.Empty : worksheet.Cells[row, 22].Value.ToString(),
                        SwiftCode = worksheet.Cells[row, 23].Value == null ? string.Empty : worksheet.Cells[row, 23].Value.ToString(),
                        BankAccountAddress = worksheet.Cells[row, 24].Value == null ? string.Empty : worksheet.Cells[row, 24].Value.ToString(),
                        Note = worksheet.Cells[row, 25].Value == null ? string.Empty : worksheet.Cells[row, 25].Value.ToString(),
                    };
                    list.Add(stage);
                }
                var data = catPartnerService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }
    }
}