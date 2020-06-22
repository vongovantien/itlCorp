using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.Authorize;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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

        private readonly IMapper mapper;
        public CatContractController(IStringLocalizer<LanguageSub> localizer, ICatContractService service, ICatPartnerService partnerSv, IMapper iMapper)
        {
            stringLocalizer = localizer;
            catContractService = service;
            mapper = iMapper;
            partnerService = partnerSv;
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
        public IActionResult Post(CatContractModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.Id = Guid.NewGuid();
            string messageDuplicate = string.Empty;
            bool checkExist = catContractService.Any(x => x.SaleService == model.SaleService  && x.OfficeId == model.OfficeId && x.PartnerId == model.PartnerId);
            if (checkExist)
            {
                messageDuplicate = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                return BadRequest(new ResultHandle { Status = false, Message = messageDuplicate });
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

        /// <summary>
        /// check existed office and service
        /// </summary>
        /// <param name="model">object to check</param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckExisted")]
        public IActionResult CheckExisted(CatContractEditModel model)
        {
      
            string messageDuplicate = string.Empty;
            bool checkExist = catContractService.Any(x => x.SaleService == model.Service && x.OfficeId == model.Office && x.PartnerId == model.PartnerId && x.SaleManId == model.SaleManId);
            if (checkExist)
            {
                //messageDuplicate = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                return BadRequest(new ResultHandle { Status = false, Message = "Duplicate service, office with sale man!" });
            }
            ResultHandle result = new ResultHandle { Data = checkExist };
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
        public IActionResult Put(CatContractModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var saleman = mapper.Map<CatContractModel>(model);
            var hs = catContractService.Update(saleman);
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
                objPartner.SalePersonId = catContractService.Get(x=>x.PartnerId == partnerId)?.OrderBy(x=>x.DatetimeCreated).FirstOrDefault().SaleManId.ToString();
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
        public async Task<IActionResult> UploadFileContract(List<IFormFile> files, [Required]string partnerId, [Required]string contractId)
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



    }
}