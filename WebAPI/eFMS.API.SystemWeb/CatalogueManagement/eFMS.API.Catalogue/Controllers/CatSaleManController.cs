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
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
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
    public class CatSaleManController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatSaleManService catSaleManService;

        private readonly IMapper mapper;
        public CatSaleManController(IStringLocalizer<LanguageSub> localizer, ICatSaleManService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            catSaleManService = service;
            mapper = iMapper;
        }

        /// <summary>
        /// get all saleman
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetAll()
        {
            var results = catSaleManService.GetSaleMan();
            return Ok(results);
        }


 
        /// <summary>
        /// get the list of sale man
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Query")]
        public IActionResult Get(CatSalemanCriteria criteria)
        {
            var results = catSaleManService.Query(criteria);
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
        public IActionResult Get(CatSalemanCriteria criteria, int page, int size)
        {
            var data = catSaleManService.Paging(criteria, page, size, out int rowCount);
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
            var results = catSaleManService.GetBy(partnerId.Trim());
            return Ok(results);
        }


        /// <summary>
        /// add new saleman
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public IActionResult Post(CatSaleManEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string messageDuplicate = string.Empty;
            bool checkExist = catSaleManService.Any(x => x.Service == model.Service  && x.Office == model.Office && x.PartnerId == model.PartnerId);
            if (checkExist)
            {
                messageDuplicate = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                return BadRequest(new ResultHandle { Status = false, Message = messageDuplicate });
            }
            var saleman = mapper.Map<CatSaleManModel>(model);
            var hs = catSaleManService.Add(saleman);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
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
        public IActionResult CheckExisted(CatSaleManEditModel model)
        {
      
            string messageDuplicate = string.Empty;
            bool checkExist = catSaleManService.Any(x => x.Service == model.Service && x.Office == model.Office);
            if (checkExist)
            {
                messageDuplicate = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                return BadRequest(new ResultHandle { Status = false, Message = messageDuplicate });
            }
            ResultHandle result = new ResultHandle { Data = false };
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
        public IActionResult Put(string id, CatSaleManEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var saleman = mapper.Map<CatSaleManModel>(model);
            var hs = catSaleManService.Update(saleman);
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

        public IActionResult Delete(Guid id)
        {
            var hs = catSaleManService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


    }
}