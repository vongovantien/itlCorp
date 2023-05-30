using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.CataloguePartner;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ESBUrl> _webUrl;
        private readonly BravoLoginModel loginInfo;
        private readonly IActionFuncLogService actionFuncLogService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatPartnerService</param>
        /// <param name="iMapper">inject interface IMapper</param>
        /// <param name="hostingEnvironment"></param>
        public CatPartnerController(IStringLocalizer<LanguageSub> localizer,
            ICatPartnerService service,
            IMapper iMapper,
            IHostingEnvironment hostingEnvironment,
            IOptions<ESBUrl> webUrl,
              IActionFuncLogService actionFuncLog)
        {
            stringLocalizer = localizer;
            catPartnerService = service;
            mapper = iMapper;
            _hostingEnvironment = hostingEnvironment;
            _webUrl = webUrl;
            actionFuncLogService = actionFuncLog;
            loginInfo = new BravoLoginModel
            {
                UserName = "bravo",
                Password = "br@vopro"
            };
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
        [Authorize]
        public IActionResult Get(CatPartnerCriteria criteria, int page, int size)
        {
            var data = catPartnerService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get partner by id
        /// </summary>
        /// <param name="criteria">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryExport")]
        [Authorize]
        public IActionResult QueryExport(CatPartnerCriteria criteria)
        {
            var results = catPartnerService.QueryExport(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("QueryExportAgreement")]
        [Authorize]
        public IActionResult QueryExportAgreement(CatPartnerCriteria criteria)
        {
            var results = catPartnerService.QueryExportAgreement(criteria);
            return Ok(results);
        }
        /// <summary>
        /// get partner by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(string id)
        {
            var data = catPartnerService.GetDetail(id);
            return Ok(data);
        }
        /// <summary>
        /// check permission
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>

        [HttpGet("CheckPermission/{id}")]
        [Authorize]
        public IActionResult CheckDetailPermission(string id)
        {
            var hs = catPartnerService.CheckDetailPermission(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            result.Data = hs.Code;
            return Ok(result);
        }

        /// <summary>
        /// check permission
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>

        [HttpGet("CheckPermissionDelete/{id}")]
        public IActionResult CheckDeletePermission(string id)
        {
            var hs = catPartnerService.CheckDeletePermission(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            result.Data = hs.Code;
            return Ok(result);
        }

        /// <summary>
        /// check tax code
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CheckTaxCode")]
        public IActionResult CheckTaxCode(CatPartnerValidateTaxCodeModel model)
        {
            string refNo = model.InternalReferenceNo == null ? "" : model.InternalReferenceNo.Trim().ToLower();
            if (string.IsNullOrEmpty(model.Id))
            {
                var result = catPartnerService.Get(x => !string.IsNullOrEmpty(x.TaxCode) && !string.IsNullOrEmpty(model.TaxCode) && x.TaxCode.Trim() == model.TaxCode.Trim().ToLower()
                                                    && (
                                                      ((string.IsNullOrEmpty(x.InternalReferenceNo) ? "" : x.InternalReferenceNo.Trim()) == refNo)
                                                      || refNo.Length == 0)
                            )?.Where(y => (string.IsNullOrEmpty(y.InternalReferenceNo) ? "" : y.InternalReferenceNo) == refNo).FirstOrDefault();
                return Ok(result);
            }
            else
            {
                var result = catPartnerService.Get(x => !string.IsNullOrEmpty(x.TaxCode) && !string.IsNullOrEmpty(model.TaxCode) && x.TaxCode.Trim() == model.TaxCode.Trim().ToLower() && x.Id != model.Id
                                                      && (
                                                      ((string.IsNullOrEmpty(x.InternalReferenceNo) ? "" : x.InternalReferenceNo.Trim()) == refNo)
                                                      || refNo.Length == 0))
                            ?.Where(y => (string.IsNullOrEmpty(y.InternalReferenceNo) ? "" : y.InternalReferenceNo) == refNo).FirstOrDefault();
                return Ok(result);
            }
        }

        /// <summary>
        /// reject partner comment
        /// </summary>
        /// <param name="partnerId">id of data that need to retrieve</param>
        /// <param name="comment">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("RejectComment")]
        [Authorize]
        public IActionResult RejectComment(string partnerId, string comment)
        {
            bool result = catPartnerService.SendMailRejectComment(partnerId, comment);
            return Ok(result);
        }


        /// <summary>
        /// Send Request Approval
        /// </summary>
        /// <param name="partnerId">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("RequestApproval")]
        [Authorize]
        public IActionResult RequestApproval(string partnerId)
        {
            var data = catPartnerService.Get(x => x.Id == partnerId).FirstOrDefault();
            bool result = catPartnerService.SendMailCreatedSuccess(data);
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
            if (!String.IsNullOrEmpty(model.InternalReferenceNo))
            {
                model.AccountNo = model.TaxCode + "." + model.InternalReferenceNo;
            }
            else
            {
                model.AccountNo = model.TaxCode;
            }
            model.Id = Guid.NewGuid().ToString();
            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            List<string> idsContract = null;
            if (model.Contracts?.Count() > 0)
            {

                model.Contracts.ForEach(x => x.Id = Guid.NewGuid());
                idsContract = model.Contracts.Select(t => t.Id.ToString()).ToList();
                model.idsContract = idsContract;

            }
            var partner = mapper.Map<CatPartnerModel>(model);
            partner.idsContract = model.idsContract;
            foreach (var item in partner.Contracts)
            {
                foreach (var it in model.Contracts)
                {
                    if (item.Id == it.Id)
                    {
                        item.IsRequestApproval = it.IsRequestApproval;
                    }
                }
            }

            var result = catPartnerService.Add(partner);

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

            if (!String.IsNullOrEmpty(model.InternalReferenceNo))
            {
                model.AccountNo = model.TaxCode + "." + model.InternalReferenceNo;
            }
            else
            {
                model.AccountNo = model.TaxCode;
            }
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
                if (model.AccountNo != null)
                {
                    if (catPartnerService.Any(x => x.AccountNo == model.AccountNo))
                    {
                        message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                    }
                }
            }
            else
            {
                if (model.AccountNo != null)
                {
                    if (catPartnerService.Any(x => x.AccountNo == model.AccountNo && x.Id != id))
                    {
                        message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                    }
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
            var hs = catPartnerService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }

        /// <summary>
        /// import list partner
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("importCustomerAgent/{type}")]
        [Authorize]
        public IActionResult ImportCustomerAgent([FromBody] List<CatPartnerImportModel> data, string type)
        {
            var hs = catPartnerService.ImportCustomerAgent(data, type);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.CatPartner.ExcelImportFileName + Templates.ExcelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
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
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcelCommercial")]
        public async Task<ActionResult> DownloadExcelCommercial()
        {
            string fileName = Templates.CatPartner.ExelImportCommercialCustomerFileName + Templates.ExcelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
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
                    var partner = new CatPartnerImportModel
                    {
                        IsValid = true,
                        ShortName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        PartnerNameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        PartnerNameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        PartnerLocation = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        TaxCode = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        InternalReferenceNo = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        PartnerMode = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        InternalCode = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        AcReference = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        PartnerGroup = worksheet.Cells[row, 10].Value?.ToString().Trim(),
                        CoLoaderCode = worksheet.Cells[row, 11].Value?.ToString().Trim(),
                        CreditPayment = worksheet.Cells[row, 12].Value?.ToString().Trim(),
                        AddressShippingEn = worksheet.Cells[row, 13].Value?.ToString().Trim(),
                        AddressShippingVn = worksheet.Cells[row, 14].Value?.ToString().Trim(),
                        CountryShipping = worksheet.Cells[row, 15].Value?.ToString().Trim(),
                        CityShipping = worksheet.Cells[row, 16].Value?.ToString().Trim(),
                        ZipCodeShipping = worksheet.Cells[row, 17].Value?.ToString().Trim(),
                        AddressEn = worksheet.Cells[row, 18].Value?.ToString().Trim(),
                        AddressVn = worksheet.Cells[row, 19].Value?.ToString().Trim(),
                        CountryBilling = worksheet.Cells[row, 20].Value?.ToString().Trim(),
                        CityBilling = worksheet.Cells[row, 21].Value?.ToString().Trim(),
                        ZipCode = worksheet.Cells[row, 22].Value?.ToString().Trim(),
                        ContactPerson = worksheet.Cells[row, 23].Value?.ToString().Trim(),
                        Tel = worksheet.Cells[row, 24].Value?.ToString().Trim(),
                        Fax = worksheet.Cells[row, 25].Value?.ToString().Trim(),
                        WorkPhoneEx = worksheet.Cells[row, 26].Value?.ToString().Trim(),
                        Email = worksheet.Cells[row, 27].Value?.ToString().Trim(),
                        BillingEmail = worksheet.Cells[row, 28].Value?.ToString(),
                        BillingPhone = worksheet.Cells[row, 29].Value?.ToString(),
                        BankAccountNo = worksheet.Cells[row, 30].Value?.ToString().Trim(),
                        BankAccountName = worksheet.Cells[row, 31].Value?.ToString().Trim(),
                        BankAccountAddress = worksheet.Cells[row, 32].Value?.ToString().Trim(),
                        SwiftCode = worksheet.Cells[row, 33].Value?.ToString().Trim(),
                        RoundUpMethod = worksheet.Cells[row, 34].Value?.ToString().Trim(),
                        ApplyDim = worksheet.Cells[row, 35].Value?.ToString().Trim(),
                        Note = worksheet.Cells[row, 36].Value?.ToString().Trim(),
                    };
                    list.Add(partner);
                }
                var data = catPartnerService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadFileCustomerAgent")]
        public IActionResult UploadFileCustomerAgent(IFormFile uploadedFile)
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
                    var partner = new CatPartnerImportModel
                    {
                        IsValid = true,
                        ShortName = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        PartnerNameEn = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        PartnerNameVn = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        PartnerLocation = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        TaxCode = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        InternalReferenceNo = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        AcReference = worksheet.Cells[row, 7].Value?.ToString(),
                        AddressShippingEn = worksheet.Cells[row, 8].Value?.ToString().Trim(),
                        AddressShippingVn = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        CountryShipping = worksheet.Cells[row, 10].Value?.ToString().Trim(),
                        CityShipping = worksheet.Cells[row, 11].Value?.ToString().Trim(),
                        ZipCodeShipping = worksheet.Cells[row, 12].Value?.ToString().Trim(),
                        AddressEn = worksheet.Cells[row, 13].Value?.ToString().Trim(),
                        AddressVn = worksheet.Cells[row, 14].Value?.ToString().Trim(),
                        CountryBilling = worksheet.Cells[row, 15].Value?.ToString().Trim(),
                        CityBilling = worksheet.Cells[row, 16].Value?.ToString().Trim(),
                        ZipCode = worksheet.Cells[row, 17].Value?.ToString().Trim(),
                        ContactPerson = worksheet.Cells[row, 18].Value?.ToString().Trim(),
                        Tel = worksheet.Cells[row, 19].Value?.ToString().Trim(),
                        Fax = worksheet.Cells[row, 20].Value?.ToString().Trim(),
                        WorkPhoneEx = worksheet.Cells[row, 21].Value?.ToString().Trim(),
                        Email = worksheet.Cells[row, 22].Value?.ToString().Trim(),
                        BillingEmail = worksheet.Cells[row, 23].Value?.ToString().Trim(),
                        BillingPhone = worksheet.Cells[row, 24].Value?.ToString().Trim(),
                    };
                    list.Add(partner);
                }
                var data = catPartnerService.CheckValidCustomerAgentImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }


        [HttpPost("GetMultiplePartnerGroup")]
        public IActionResult GetMultiplePartnerGroup(PartnerMultiCriteria criteria)
        {
            var data = catPartnerService.GetMultiplePartnerGroup(criteria);
            return Ok(data);
        }

        [HttpPost("GetPartnerForKeyingCharge")]
        public IActionResult GetPartnerForKeyingCharge(PartnerMultiCriteria criteria)
        {
            var data = catPartnerService.GetPartnerForKeyinCharge(criteria);
            return Ok(data);
        }

        /// <summary>
        /// get the list of partners
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List branch sub of partner</returns>
        [HttpGet]
        [Route("GetSubListPartnerByID/{id}")]
        public IActionResult GetSubListPartnerByID(string id)
        {
            var results = catPartnerService.GetSubListPartnerByID(id.Trim());
            return Ok(results);
        }

        /// <summary>
        /// Update info for partner
        /// </summary>
        /// <param name="criteria">CatPartnerCriteria</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateInfoForPartner")]
        public IActionResult UpdateInfoForPartner(CatPartnerEditModel model)
        {
            var partner = mapper.Map<CatPartnerModel>(model);
            var hs = catPartnerService.UpdatePartnerData(partner);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetListSaleman")]
        [Authorize]
        public IActionResult GetListSaleman(string partnerId, string transactionType, string shipmentType, string office = null)
        {
            var data = catPartnerService.GetListSaleman(partnerId, transactionType, shipmentType, office);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetPartnerByTaxCode/{taxCode}")]
        [Authorize]
        public async Task<IActionResult> GetPartnerByTaxCode(string taxCode)
        {
            var results = await catPartnerService.GetPartnerByTaxCode(taxCode);
            return Ok(results);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddPartnerFromUserData")]
        [Authorize]
        public async Task<IActionResult> AddPartnerFromUserData(Guid userId)
        {
            var hs = await catPartnerService.AddPartnerFromUserData(userId);
            if (hs.Success)
            {
                var message = HandleError.GetMessage(hs, Crud.Insert);
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                return Ok(result);
            }
            return Ok(hs);
        }
        
        /// Get AC ref partner with same saleman
        /// </summary>
        /// <param name="salemanId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetParentPartnerSameSaleman")]
        public IActionResult GetParentPartnerSameSaleman(CatPartnerCriteria criteria)
        {
            var data = catPartnerService.GetParentPartnerSameSaleman(criteria);
            return Ok(data);
        }
        
        [HttpPut("SyncPartnerToAccountantSystem")]
        [Authorize]
        public async Task<IActionResult> GetListPartnerToSync(List<RequestStringListModel> request)
        {
            var _startDateProgress = DateTime.Now;
            if (!ModelState.IsValid) return BadRequest();

            try
            {
                // 1. Login
                HttpResponseMessage responseFromApi = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api/Login", loginInfo, null);
                BravoLoginResponseModel loginResponse = responseFromApi.Content.ReadAsAsync<BravoLoginResponseModel>().Result;
                if (loginResponse?.Success != "1")
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer["MSG_SYNC_FAIL"].Value, Data = null });
                }
                // 2. Get Data To Sync.
                List<string> ids = request.Select(x => x.Id).ToList();

                List<string> idsAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                List<string> idsUpdate = request.Where(x => x.Action == ACTION.UPDATE).Select(x => x.Id).ToList();

                List<PartnerSyncModel> listAdd = (idsAdd.Count > 0) ? await catPartnerService.GetListPartnerToSync(idsAdd) : new List<PartnerSyncModel>();
                List<PartnerSyncModel> listUpdate = (idsUpdate.Count > 0) ? await catPartnerService.GetListPartnerToSync(idsUpdate) : new List<PartnerSyncModel>();

                HttpResponseMessage resAdd = new HttpResponseMessage();
                HttpResponseMessage resUpdate = new HttpResponseMessage();
                BravoResponseModel responseAddModel = new BravoResponseModel();
                BravoResponseModel responseUpdateModel = new BravoResponseModel();

                // 3. Call Bravo to SYNC.
                if (listAdd.Count > 0)
                {
                    resAdd = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSCustomerSyncAdd", listAdd, loginResponse.TokenKey);
                    responseAddModel = await resAdd.Content.ReadAsAsync<BravoResponseModel>();

                    #region -- Ghi Log --
                    var modelLog = new SysActionFuncLogModel
                    {
                        Id = Guid.NewGuid(),
                        FuncLocal = "GetListPartnerToSync",
                        FuncPartner = "EFMSCustomerSyncAdd",
                        ObjectRequest = JsonConvert.SerializeObject(listAdd),
                        ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                        Major = "Đồng bộ đối tượng",
                        StartDateProgress = _startDateProgress,
                        EndDateProgress = DateTime.Now
                    };
                    var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                    #endregion
                }

                if (listUpdate.Count > 0)
                {
                    resUpdate = await HttpClientService.PostAPI(_webUrl.Value.Url + "/itl-bravo/Accounting/api?func=EFMSCustomerSyncUpdate", listUpdate, loginResponse.TokenKey);
                    responseUpdateModel = await resUpdate.Content.ReadAsAsync<BravoResponseModel>();

                    #region -- Ghi Log --
                    var modelLog = new SysActionFuncLogModel
                    {
                        Id = Guid.NewGuid(),
                        FuncLocal = "GetListPartnerToSync",
                        FuncPartner = "EFMSCustomerSyncUpdate",
                        ObjectRequest = JsonConvert.SerializeObject(listAdd),
                        ObjectResponse = JsonConvert.SerializeObject(responseAddModel),
                        Major = "Đồng bộ đối tượng",
                        StartDateProgress = _startDateProgress,
                        EndDateProgress = DateTime.Now
                    };
                    var hsAddLog = actionFuncLogService.AddActionFuncLog(modelLog);
                    #endregion
                }
                //update partner status
                if (responseAddModel.Success == "1" || responseAddModel.Success == "2")
                {
                    var idsItemAdd = request.Where(x => x.Action == ACTION.ADD).Select(x => x.Id).ToList();
                    var listUpdated = await catPartnerService.GetAsync(x => idsItemAdd.Contains(x.Id));

                    listUpdated.ForEach(x => x.SysMappingId = x.AccountNo);
                    var hs = await catPartnerService.UpdatePartnerSyncStatus(listUpdated);
                }

                if (responseAddModel?.Success == "1" || responseUpdateModel?.Success == "1")
                {
                    ResultHandle result = new ResultHandle { Status = true, Message = stringLocalizer["MSG_SYNC_SUCCESS"].Value, Data = ids };
                    return Ok(result);
                }
                else
                {
                    if (responseAddModel?.Success == null && responseUpdateModel?.Success == null)
                    {
                        return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer["MSG_SYNC_FAIL"].Value, Data = null });
                    }
                    return BadRequest(new ResultHandle { Status = false, Message = responseAddModel.Msg + "\n" + responseUpdateModel.Msg, Data = ids });
                }
            }
            catch (Exception)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer["MSG_SYNC_FAIL"].Value, Data = null });
            }
        }
    }
}