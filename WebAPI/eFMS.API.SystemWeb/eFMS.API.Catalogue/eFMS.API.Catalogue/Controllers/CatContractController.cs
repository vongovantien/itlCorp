﻿using System;
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
        public IActionResult GetBy(string partnerId)
        {
            var results = catContractService.GetBy(partnerId.Trim());
            return Ok(results);
        }

        /// <summary>
        /// get the list of saleman by partner id
        /// </summary>
        /// <param name="partnerId">partnerId that want to retrieve saleman</param>
        /// <returns></returns>

        [HttpGet("GetSalemanIdByPartnerId/{partnerId}")]
        public IActionResult GetSalemanIdByPartnerId(string partnerId)
        {
            Guid? id = catContractService.GetContractIdByPartnerId(partnerId);
            return Ok(id);
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
            model.Id = Guid.NewGuid();
            string messageExisted = CheckExistedContract(model);
            if (!string.IsNullOrEmpty(messageExisted))
            {
                return BadRequest(new ResultHandle { Status = false, Message = messageExisted });
            }
            var hs = catContractService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        ///// <summary>
        ///// check existed office and service
        ///// </summary>
        ///// <param name="model">object to check</param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("CheckExisted")]
        //public IActionResult CheckExisted(CatContractModel model)
        //{
      
        //    string messageDuplicate = string.Empty;
        //    bool existed = false;
        //    if(model.Id != Guid.Empty)
        //    {
        //        existed = catContractService.Any(x => x.ContractNo == model.ContractNo);
        //    }
        //    else
        //    {

        //    }
        //    if (existed)
        //    {
        //        return BadRequest(new ResultHandle { Status = false, Message = "Contract no has been existed!" });
        //    }
        //    ResultHandle result = new ResultHandle { Data = existed };
        //    return Ok(result);
        //}

        private string CheckExistedContract(CatContractModel model)
        {
            string messageDuplicate = string.Empty;
            var office = model.OfficeId.Split(";").ToArray();
            if (model.Id != Guid.Empty)
            {
                if(model.ContractType != "Official")
                {
                    if (catContractService.Any(x => x.SaleService == model.SaleService && office.Contains(x.OfficeId.ToLower()) &&  x.Id != model.Id && x.PartnerId == model.PartnerId))
                    {
                        messageDuplicate = "Duplicate service, office, salesman!";
                    }
                }
                else
                {
                    if (catContractService.Any(x => x.SaleService == model.SaleService && office.Contains(x.OfficeId.ToLower()) && x.ContractNo  == model.ContractNo && !string.IsNullOrEmpty(model.ContractNo) && x.Id != model.Id && x.PartnerId == model.PartnerId))
                    {
                        messageDuplicate = "Contract no has been existed!";
                    }
                }
                //if(catContractService.Any(x => x.ContractNo == model.ContractNo && x.Id != model.Id && !string.IsNullOrEmpty(model.ContractNo)))
                //{
                //    existed = true;
                //}
            }
            else
            {
                //existed = catContractService.Any(x => x.ContractNo == model.ContractNo && !string.IsNullOrEmpty( model.ContractNo));
                if (model.ContractType != "Official")
                {
                    if (catContractService.Any(x => x.SaleService == model.SaleService && office.Contains(x.OfficeId.ToLower()) && x.PartnerId == model.PartnerId))
                    {
                        messageDuplicate = "Duplicate service, office, salesman!";
                    }
                }
                else
                {
                    if (catContractService.Any(x => x.SaleService == model.SaleService && office.Contains(x.OfficeId.ToLower()) && x.ContractNo == model.ContractNo && !string.IsNullOrEmpty(model.ContractNo) && x.PartnerId == model.PartnerId))
                    {
                        messageDuplicate = "Contract no has been existed!";
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
                objPartner.SalePersonId = catContractService.Get(x=>x.PartnerId == partnerId)?.OrderBy(x=>x.DatetimeCreated).FirstOrDefault()?.SaleManId.ToString();
                var hsPartner = partnerService.Update(objPartner, x=>x.Id == partnerId);
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
                FolderName = folderName,
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
        public async Task<IActionResult> UploadFileMoreContract(List<IFormFile> files,string contractIds,string partnerId)
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
        public IActionResult ActiveInactiveContract(Guid id,string partnerId,[FromBody]SalesmanCreditModel credit)
        {
            var hs = catContractService.ActiveInActiveContract(id, partnerId,credit);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
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
            string fileName = Templates.CatContract.ExelImportFileName + Templates.ExelImportEx;
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
                    string dateEffect = worksheet.Cells[row, 7].Value?.ToString().Trim();
                    DateTime? dateToPase = null;
                    if (DateTime.TryParse(dateEffect, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPase = DateTime.Parse(temp.ToString("dd/MM/yyyy"), culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        if(dateEffect != null) {
                            dateToPase = DateTime.Parse(dateEffect, culture);
                        }
                    }

                    string dateExpired = worksheet.Cells[row, 8].Value?.ToString().Trim();
                    DateTime? dateToPaseExpired = null;
                    if (DateTime.TryParse(dateExpired, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPaseExpired = DateTime.Parse(temp.ToString("dd/MM/yyyy"), culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        if(dateExpired != null)
                        {
                            dateToPaseExpired = DateTime.Parse(dateExpired, culture);
                        }
                    }


                    var contract = new CatContractImportModel
                    {
                        IsValid = true,
                        CustomerId = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        ContractNo = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        ContractType = worksheet.Cells[row, 3].Value?.ToString().Trim(),
                        SaleService = worksheet.Cells[row, 4].Value?.ToString().Trim(), 
                        Company = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                        Office  = worksheet.Cells[row, 6].Value?.ToString().Trim(),
                        EffectDate = !string.IsNullOrEmpty(dateEffect) ? dateToPase : (DateTime?)null,
                        ExpireDate = !string.IsNullOrEmpty(dateExpired) ? dateToPaseExpired : (DateTime?)null,
                        PaymentMethod = worksheet.Cells[row, 9].Value?.ToString().Trim(),
                        Vas = worksheet.Cells[row, 10].Value?.ToString().Trim(), 
                        Salesman = worksheet.Cells[row, 11].Value?.ToString().Trim(),
                        PaymentTermTrialDay = worksheet.Cells[row, 12].Value?.ToString().Trim(),
                        CreditLimited =  worksheet.Cells[row, 13].Value?.ToString().Trim(),
                        CreditLimitedRated = worksheet.Cells[row, 14].Value?.ToString().Trim(),
                        Status = worksheet.Cells[row, 15].Value?.ToString().Trim()
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

    }
}