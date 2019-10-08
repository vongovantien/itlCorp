using System;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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
        private readonly ICurrentUser currentUser;

        public SysBuController(IStringLocalizer<LanguageSub> localizer, ISysBuService sysBuService, 
            IMapper mapper,
            ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            this.sysBuService = sysBuService;
            this.mapper = mapper;
            currentUser = currUser;
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
            var checkExistMessage = CheckExistCode(model.CompanyCode);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = sysBuService.Add(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBy(Guid id)
        {
            var result = sysBuService.First(x => x.Id == id);
            if (result == null)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Không tìm thấy Company", Data = result });
            }
            else
            {
                return Ok(new ResultHandle { Status = true, Message = "Success", Data = result });
            }
        }

        [HttpPut]
        [Route("Update")]
        public IActionResult Update(SysBuAddModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = sysBuService.Update(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

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
        public IActionResult Delete(Guid id)
        {
            var hs = sysBuService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExistCode(string Code)
        {
            string message = string.Empty;
            if (Code != "" || Code != null)
            {
                if (sysBuService.Any(x => (x.Code == Code)))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            return message;
        }
    }
}