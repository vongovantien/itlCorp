using System;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatChartOfAccountsController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatChartOfAccountsService catChartAccountsService;
        private readonly ICurrentUser currentUser;

        public CatChartOfAccountsController(IStringLocalizer<LanguageSub> localizer, ICatChartOfAccountsService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            catChartAccountsService = service;
            currentUser = user;
        }

        [HttpPost("Query")]
        [Authorize]
        public IActionResult Query(CatChartOfAccountsCriteria criteria)
        {
            var data = catChartAccountsService.Query(criteria);
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CatChartOfAccountsCriteria criteria, int page, int size)
        {
            var data = catChartAccountsService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public IActionResult Get(Guid id)
        {
            var data = catChartAccountsService.GetDetail(id);
            return Ok(data);
        }

        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CatChartOfAccounts model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.AccountCode, Guid.Empty);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChartAccountsService.Add(model);
            return Ok(hs);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CatChartOfAccounts model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.AccountCode, model.Id);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catChartAccountsService.Update(model);
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
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catChartAccountsService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(string accountNo, Guid id)
        {
            string message = string.Empty;
            if (id != Guid.Empty)
            {
                if (catChartAccountsService.Any(x => x.AccountCode != null && x.AccountCode == accountNo && x.Id != id))
                {
                    message = "Account Code is existed !";
                }
            }
            else
            {
                if (catChartAccountsService.Any(x => x.AccountCode != null && x.AccountCode == accountNo))
                {
                    message = "Account Code is existed !";
                }
            }
            return message;
        }
    }
}