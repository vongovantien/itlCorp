using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.Authorize;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatContractController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatContractService catContractService;
        private readonly ICatPartnerService partnerService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ApiServiceUrl> apiServiceUrl;

        private readonly IMapper mapper;
        public CatContractController(IStringLocalizer<LanguageSub> localizer,
            ICatContractService service,
            ICatPartnerService partnerSv,
            IOptions<ApiServiceUrl> serviceUrl,
            IMapper iMapper,
            IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catContractService = service;
            mapper = iMapper;
            partnerService = partnerSv;
            _hostingEnvironment = hostingEnvironment;
            apiServiceUrl = serviceUrl;
        }

        /// <summary>
        /// get all saleman
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var results = catContractService.GetContracts();
            return Ok(results);
        }

        /// <summary
        /// get the list of sale man
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatContractCriteria criteria)
        {
            var results = catContractService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of sale man
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [AuthorizeEx(Menu.catPartnerdata, UserPermission.AllowAccess)]
        public IActionResult Get(CatContractCriteria criteria, int page, int size)
        {
            var data = catContractService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
        /// <summary>
        /// get the list of saleman by partner id
        /// </summary>
        /// <param name="partnerId">partnerId that want to retrieve saleman</param>
        /// <returns></returns>

        [HttpGet("GetBy")]
        [Authorize]
        public IActionResult GetBy(string partnerId, bool? all)
        {
            var results = catContractService.GetBy(partnerId.Trim(), all);
            return Ok(results);
        }

        /// <summary>
        /// get the list of saleman by partner id
        /// </summary>
        /// <param name="partnerId">partnerId that want to retrieve saleman</param>
        /// <param name="jobId">partnerId that want to retrieve saleman</param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GetSalemanIdByPartnerId/{partnerId}/{jobId}")]
        public IActionResult GetSalemanIdByPartnerId(string partnerId, string jobId)
        {
            object data = catContractService.GetContractIdByPartnerId(partnerId, jobId);
            return Ok(data);
        }
        /// <summary>
        /// add new saleman
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatContractModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string messageExisted = catContractService.CheckExistedContract(model);
            if (!string.IsNullOrEmpty(messageExisted))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageExisted });
            }
            model.Id = Guid.NewGuid();
            var hs = catContractService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

       
        /// <summary>
        /// add new saleman
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("CustomerRequest")]
        [Authorize]
        public IActionResult CustomerRequest(CatContractModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string messageExisted = catContractService.CheckExistedContract(model);
            if (!string.IsNullOrEmpty(messageExisted))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageExisted });
            }
            model.Id = Guid.NewGuid();
            var hs = catContractService.CustomerRequest(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
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
        [Route("Update")]
        [Authorize]
        [HttpPut]
        public IActionResult Put(CatContractModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string messageExisted = catContractService.CheckExistedContract(model);
            if (!string.IsNullOrEmpty(messageExisted))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageExisted });
            }
            if (model.isChangeAgrmentType == true)
            {
                model.Active = false;
            }
         
            string msgCheckUpdateSalesman = CheckUpdateContract(model);
            if (!string.IsNullOrEmpty(msgCheckUpdateSalesman))
            {
                return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateSalesman, Data = new { errorCode = 403 } });
            }
            
            var isChangeAgrmentType = false;
            var hs = catContractService.Update(model, out isChangeAgrmentType);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                if (isChangeAgrmentType == true)
                {
                    Response.OnCompleted(async () =>
                    {
                        // Update due date invoice và công nợ quá hạn
                        await UpdateDueDateAndOverDaysAfterChangePaymentTerm(model);
                    });
                }
            }
            return Ok(result);
        }

        /// <summary>
        /// Call function update due date và tính công nợ quá hạn
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task<HandleState> UpdateDueDateAndOverDaysAfterChangePaymentTerm(CatContractModel model)
        {
            Uri urlAccounting = new Uri(apiServiceUrl.Value.ApiUrlAccounting);
            string accessToken = Request.Headers["Authorization"].ToString();

            HttpResponseMessage resquest = await HttpClientService.PostAPI(urlAccounting + "/api/v1/e/AccountReceivable/UpdateDueDateAndOverDaysAfterChangePaymentTerm", model, accessToken);
            var response = await resquest.Content.ReadAsAsync<HandleState>();
            return response;
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="id">id of data that need to delete</param>
        /// <param name="partnerId">id of data that need to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}/{partnerId}")]
        [Authorize]
        public IActionResult Delete(Guid id, string partnerId)
        {
            var hs = catContractService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (hs.Success)
            {
                var objPartner = partnerService.Get(x => x.Id == partnerId).FirstOrDefault();
                objPartner.SalePersonId = catContractService.Get(x => x.PartnerId == partnerId)?.OrderBy(x => x.DatetimeCreated).FirstOrDefault()?.SaleManId.ToString();
                var hsPartner = partnerService.Update(objPartner, x => x.Id == partnerId);
                if (!hsPartner.Success)
                {
                    return BadRequest();
                }
            }
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// attach multi files to shipment
        /// </summary>
        /// <param name="files"></param>
        /// <param name="partnerId"></param>
        /// <param name="contractId"></param>
        /// 
        /// <returns></returns>
        [HttpPut("UploadFile/{PartnerId}/{contractId}")]
        [Authorize]
        public async Task<IActionResult> UploadFileContract(IFormFile files, [Required]string partnerId, [Required]string contractId)
        {
            ContractFileUploadModel model = new ContractFileUploadModel
            {
                Files = files,
                PartnerId = partnerId,
                ChildId = contractId
            };

            var result = await catContractService.UploadContractFile(model);
            return Ok(result);
        }

        /// <summary>
        /// attach file for more contract
        /// </summary>
        /// <param name="files"></param>
        /// <param name="partnerId"></param>
        /// <param name="contractIds"></param>
        /// 
        /// <returns></returns>
        [HttpPut("UploadFileMoreContract/{PartnerId}/{contractIds}")]
        [Authorize]
        public async Task<IActionResult> UploadFileMoreContract(List<IFormFile> files, string contractIds, string partnerId)
        {
            string folderName = Request.Headers["Module"];
            ResultHandle result = new ResultHandle();
            List<string> contractIdlst = contractIds.Split(',').ToList();
            List<ContractFileUploadModel> lst = new List<ContractFileUploadModel>();
            int i = 0;
            foreach (var item in contractIdlst)
            {
                ContractFileUploadModel model = new ContractFileUploadModel
                {
                    ChildId = item,
                    PartnerId = partnerId,
                    Files = files[i]
                };
                lst.Add(model);
                i++;
            }
            result = await catContractService.UploadMoreContractFile(lst);
            return Ok(result);
        }


        [HttpGet("GetById")]
        [Authorize]
        public IActionResult GetById(Guid Id)
        {
            var results = catContractService.GetById(Id);
            return Ok(results);
        }

        [Authorize]
        [HttpPut("UpdateFileToContract")]
        public IActionResult UpdateFilesToContract([FromBody]List<SysImage> files)
        {
            var result = catContractService.UpdateFileToContract(files);
            return Ok(result);
        }

        /// get file contract of partner
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetFileAttachsContract")]
        public IActionResult GetFileAttachsContract(string partnerId, string contractId)
        {
            var results = catContractService.GetFileContract(partnerId, contractId);
            return Ok(results);
        }

        [Authorize]
        [HttpDelete("DeleteContractAttachedFile/{id}")]
        public IActionResult DeleteAttachedFile([Required]Guid id)
        {
            var result = catContractService.DeleteFileContract(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("ActiveInactiveContract/{Id}/{partnerId}")]
        public IActionResult ActiveInactiveContract(Guid Id, string partnerId, [FromBody]SalesmanCreditModel credit)
        {
            var hs = catContractService.ActiveInActiveContract(Id, partnerId, credit, out bool active);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    if(active == true)
                    {
                        var existedContract = catContractService.CheckExistedContractInActive(Id, partnerId, out List<ServiceOfficeGroup> serviceOfficeGrps);
                        if (existedContract != null && existedContract != null)
                        {
                            string accessToken = Request.Headers["Authorization"].ToString();
                            /// cal API để chuyển công nợ
                            Uri urlAccounting = new Uri(apiServiceUrl.Value.ApiUrlAccounting);
                            var currentContract = catContractService.GetContractById(Id);
                            var model = new
                            {
                                PartnerId = partnerId,
                                FromSalesman = existedContract.SaleManId,
                                ToSalesman = currentContract.SaleManId,
                                ContractId = Id,
                                ServiceOffice = serviceOfficeGrps
                            };
                            HttpResponseMessage resquest = await HttpClientService.PutAPI(urlAccounting + "/api/v1/vi/AccountReceivable/MoveSalesmanReceivableData", model, accessToken);

                            var catContractModel = mapper.Map<CatContractModel>(currentContract);
                            await UpdateDueDateAndOverDaysAfterChangePaymentTerm(catContractModel);
                        }

                    }
                });
            }

            return Ok(result);
        }

        [HttpGet("CheckExistedContract")]
        public IActionResult CheckExistedContract(Guid id, string partnerId)
        {
            var result = catContractService.CheckExistedContractActive(id, partnerId);
            bool IsExisted = result != null && result.Count() > 0  ? true : false;
            return Ok(IsExisted);
        }

        [HttpGet("CheckExistedContractInactive")]
        public IActionResult CheckExistedContractInactive(Guid id, string partnerId)
        {
            var result = catContractService.CheckExistedContractInActive(id, partnerId, out List<ServiceOfficeGroup> serviceOfficeGrps);
            return Ok(serviceOfficeGrps);
        }



        /// <summary>
        /// send email AR Confirmed
        /// </summary>
        /// <param name="partnerId">id of data that need to retrieve</param>
        /// <param name="contractId">id of data that need to retrieve</param>
        /// <param name="partnerType">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("ARConfirmed")]
        [Authorize]
        public IActionResult ARConfirmed(string partnerId, string contractId, string partnerType)
        {
            bool result = catContractService.SendMailARConfirmed(partnerId, contractId, partnerType);
            return Ok(result);
        }



        /// <summary>
        /// reject partner comment
        /// </summary>
        /// <param name="contractId">id of data that need to retrieve</param>
        /// <param name="partnerType">id of data that need to retrieve</param>
        /// <param name="partnerId">id of data that need to retrieve</param>
        /// <param name="comment">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet("RejectComment")]
        [Authorize]
        public IActionResult RejectComment(string partnerId, string contractId, string comment, string partnerType)
        {
            bool result = catContractService.SendMailRejectComment(partnerId, contractId, comment, partnerType);
            return Ok(result);
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.CatContract.ExcelImportFileName + Templates.ExcelImportEx;
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
        /// import list partner
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<CatContractImportModel> data)
        {
            var hs = catContractService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
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
                DateTime temp;
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<CatContractImportModel> list = new List<CatContractImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string dateEffect = worksheet.Cells[row, 8].Value?.ToString().Trim();
                    DateTime? dateToPase = null;
                    if (DateTime.TryParse(dateEffect, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPase = DateTime.Parse(temp.ToString("dd/MM/yyyy"), culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        if (dateEffect != null)
                        {
                            dateToPase = DateTime.Parse(dateEffect, culture);
                        }
                    }

                    string dateExpired = worksheet.Cells[row, 9].Value?.ToString().Trim();
                    DateTime? dateToPaseExpired = null;
                    if (DateTime.TryParse(dateExpired, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPaseExpired = DateTime.Parse(temp.ToString("dd/MM/yyyy"), culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        if (dateExpired != null)
                        {
                            dateToPaseExpired = DateTime.Parse(dateExpired, culture);
                        }
                    }


                    var contract = new CatContractImportModel
                    {
                        IsValid = true,
                        CustomerId = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        Salesman = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        Company = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        Office = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        ContractNo = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        ContractType = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        SaleService = worksheet.Cells[row, 7].Value?.ToString().Trim(),
                        EffectDate = !string.IsNullOrEmpty(dateEffect) ? dateToPase : (DateTime?)null,
                        ExpireDate = !string.IsNullOrEmpty(dateExpired) ? dateToPaseExpired : (DateTime?)null,
                        PaymentMethod = worksheet.Cells[row, 10].Value?.ToString().Trim(),
                        CurrencyId = worksheet.Cells[row, 11].Value?.ToString().Trim(),
                        Vas = worksheet.Cells[row, 12].Value?.ToString().Trim(),
                        PaymentTermTrialDay = worksheet.Cells[row, 13].Value?.ToString().Trim(),
                        BaseOn = worksheet.Cells[row, 14].Value?.ToString().Trim(),
                        CreditLimited = worksheet.Cells[row, 15].Value?.ToString().Trim(),
                        CreditLimitedRated = worksheet.Cells[row, 16].Value?.ToString().Trim(),
                        Description = worksheet.Cells[row, 17].Value?.ToString().Trim()
                    };
                    list.Add(contract);
                }
                var data = catContractService.CheckValidImport(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        [HttpPost]
        [Route("QueryAgreement")]
        public IActionResult QueryAgreement(CatContractCriteria query)
        {
            IQueryable<CatAgreementModel> result = catContractService.QueryAgreement(query);
            if (result == null)
            {
                return Ok(new List<CatAgreementModel>());
            }
            return Ok(result);
        }

        private string CheckUpdateContract(CatContractModel model)
        {
            string errorMsg = string.Empty;
            var currentContract = catContractService.GetContractById(model.Id);
            if(currentContract.ContractType == "Guarantee")
            {
                if(currentContract.SaleManId != model.SaleManId)
                {
                    return "Cannot change salesman from Guarantee contract";
                }

                if(model.CurrencyId != "VND" || model.CreditCurrency != "VND")
                {
                    return "Cannot change currency from Guarantee contract";
                }
            }
            if ((model.ContractType == "Guarantee" && currentContract.ContractType != "Guarantee") 
                || (currentContract.ContractType == "Guarantee" && model.ContractType != "Guarantee"))
            {
                return string.Format("Cannot change contract type");
            }

            return errorMsg;
        }
    }
}