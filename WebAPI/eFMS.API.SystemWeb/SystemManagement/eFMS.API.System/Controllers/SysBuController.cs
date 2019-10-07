using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysBuController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysBuService sysBuService;
        private readonly IMapper mapper;

        public SysBuController(IStringLocalizer<LanguageSub> localizer, ISysBuService sysBuService, IMapper mapper)
        {
            stringLocalizer = localizer;
            this.sysBuService = sysBuService;
            this.mapper = mapper;
        }


        [HttpGet]
        public IActionResult Get()
        {
            var response = sysBuService.Get();
            return Ok(response);
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(SysBuCriteria search)
        {
            var results = sysBuService.Query(search);
            return Ok(results);
        }

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysBuCriteria search, int page, int size)
        {
            var data = sysBuService.Paging(search, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost]
        [Route("Add")]
        public IActionResult Add(SysBuAddModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            //var sysBu = mapper.Map<SysBuModel>(model);

            var hs = sysBuService.Add(model);

            var message = HandleError.GetMessage(hs, Infrastructure.Common.Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        public IActionResult Update(SysBuAddModel model)
        {

            if (!ModelState.IsValid) return BadRequest();

            //var sysBu = mapper.Map<SysBuModel>(model);

            var hs = sysBuService.Update(model);

            var message = HandleError.GetMessage(hs, Infrastructure.Common.Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        //[Authorize]
        public IActionResult Delete(int id)
        {
            var hs = sysBuService.Delete(id);
            var message = HandleError.GetMessage(hs, Infrastructure.Common.Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}