using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Common;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatPartnerController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerService catPartnerService;
        private readonly IMapper mapper;
        public CatPartnerController(IStringLocalizer<LanguageSub> localizer, ICatPartnerService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            catPartnerService = service;
            mapper = iMapper;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatPartnerCriteria criteria)
        {
            var results = catPartnerService.Query(criteria);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Get(CatPartnerCriteria criteria, int page, int size)
        {
            var data = catPartnerService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost]
        [Route("PagingCustomer")]
        public IActionResult GetCustomer(CatPartnerCriteria criteria, int page, int size)
        {
            var data = catPartnerService.PagingCustomer(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var data = catPartnerService.First(x => x.Id == id);
            return Ok(data);
        }
        [HttpPost]
        [Route("Add")]
        public IActionResult Post(CatPartnerEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var partner = mapper.Map<CatPartnerModel>(model);
            partner.UserCreated = "01";
            partner.DatetimeCreated = DateTime.Now;
            partner.Inactive = false;
            partner.PartnerGroup = PlaceTypeEx.GetPartnerGroup(model.PartnerGroup);
            var hs = catPartnerService.Add(partner);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, CatPartnerEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var partner = mapper.Map<CatPartnerModel>(model);
            partner.UserModified = "01";
            partner.DatetimeModified = DateTime.Now;
            partner.Id = id;
            partner.PartnerGroup = PlaceTypeEx.GetPartnerGroup(model.PartnerGroup);
            if (partner.Inactive == true)
            {
                partner.InactiveOn = DateTime.Now;
            }
            var hs = catPartnerService.Update(partner, x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var hs = catPartnerService.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
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
    }
}