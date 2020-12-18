using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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

        private readonly IMapper mapper;
        public CatContractController(IStringLocalizer<LanguageSub> localizer, ICatContractService service, ICatPartnerService partnerSv, IMapper iMapper, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catContractService = service;
            mapper = iMapper;
            partnerService = partnerSv;
            _hostingEnvironment = hostingEnvironment;
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
            string messageExisted = CheckExistedContract(model);
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

        private string CheckExistedContract(CatContractModel model)
        {
            string messageDuplicate = string.Empty;

            var officeIds = catContractService.Get().Select(t => t.OfficeId).ToArray();
            var saleServices = catContractService.Get().Select(t => t.SaleService).ToArray();
            var office = model.OfficeId.Split(";").ToArray();
            var sale = model.SaleService.Split(";").ToArray();
            var dataContract = catContractService.Get(x => x.PartnerId == model.PartnerId).ToList();
            var arrayOffice = new HashSet<string>(model.OfficeId.Split(';'));
            var dataCheck = model.Id != Guid.Empty ? dataContract.Where(x => (!string.IsNullOrEmpty(x.SaleService) && x.SaleService.Split(";").Any(s => sale.Contains(s))) && (!string.IsNullOrEmpty(x.OfficeId) && x.OfficeId.Split(";").Any(o => arrayOffice.Contains(o))) && (x.SaleManId != model.SaleManId || x.SaleManId == model.SaleManId) && x.Id != model.Id).ToList() :
            dataContract.Where(x => (!string.IsNullOrEmpty(x.SaleService) && x.SaleService.Split(";").Any(s => sale.Contains(s))) && (!string.IsNullOrEmpty(x.OfficeId) && x.OfficeId.Split(";").Any(o => arrayOffice.Contains(o))) && (x.SaleManId != model.SaleManId || x.SaleManId == model.SaleManId)).ToList();
            if (model.Id != Guid.Empty)
            {
                if (model.ContractType != "Official")
                {
                    if (!string.IsNullOrEmpty(model.ContractNo))
                    {
                        if (catContractService.Any(x => x.ContractNo == model.ContractNo && x.PartnerId == model.PartnerId && x.Id != model.Id))
                        {
                            messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
                        }
                    }
                    if (dataCheck.Count() > 0)
                    {
                        messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.ContractNo))
                    {
                        if (catContractService.Any(x => x.ContractNo == model.ContractNo && x.Id != model.Id && x.PartnerId == model.PartnerId))
                        {
                            messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
                        }
                    }
                    if (dataCheck.Count() > 0)
                    {
                        messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
                    }
                }
            }
            else
            {
                if (model.ContractType != "Official")
                {
                    if (!string.IsNullOrEmpty(model.ContractNo))
                    {
                        if (catContractService.Any(x => x.ContractNo == model.ContractNo && x.PartnerId == model.PartnerId))
                        {
                            messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
                        }
                    }

                    if (dataCheck.Count() > 0)
                    {
                        messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(model.ContractNo))
                    {
                        if (catContractService.Any(x => x.ContractNo == model.ContractNo && x.PartnerId == model.PartnerId))
                        {
                            messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_CONTRACT_NO_EXISTED], model.ContractNo);
                        }
                    }

                    if (dataCheck.Count() > 0)
                    {
                        messageDuplicate = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CONTRACT_DUPLICATE_SERVICE]);
                    }

                }
            }
            return messageDuplicate;
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
            string messageExisted = CheckExistedContract(model);
            if (!string.IsNullOrEmpty(messageExisted))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageExisted });
            }
            if (model.isChangeAgrmentType == true)
            {
                model.Active = false;
            }
            var hs = catContractService.Update(model);
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
            string folderName = Request.Headers["Module"];
            ContractFileUploadModel model = new ContractFileUploadModel
            {
                Files = files,
                FolderName = "Catalogue",
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
        [HttpPut("ActiveInactiveContract/{id}/{partnerId}")]
        public IActionResult ActiveInactiveContract(Guid id, string partnerId, [FromBody]SalesmanCreditModel credit)
        {
            var hs = catContractService.ActiveInActiveContract(id, partnerId, credit);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
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
    }
}