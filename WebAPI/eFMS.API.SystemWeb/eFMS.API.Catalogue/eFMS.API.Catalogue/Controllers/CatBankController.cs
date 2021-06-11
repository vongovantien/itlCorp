using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Linq;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common.Infrastructure.Common;
using System;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatBankController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatBankService CatBankService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatBankService</param>
        public CatBankController(IStringLocalizer<LanguageSub> localizer, ICatBankService service,
            ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            CatBankService = service;
            currentUser = currUser;
        }

        /// <summary>
        /// get the list of all currencies
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var data = CatBankService.GetAll()?.OrderBy(x => x.BankNameVn);
            return Ok(data);
        }

        /// <summary>
        /// get commodity by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("getById/{id}")]
        public IActionResult Get(Guid id)
        {
            var data = CatBankService.First(x => x.Id == id);
            return Ok(data);
        }

        /// <summary>
        /// get and paging the list of currencies by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("paging")]
        public IActionResult Get(CatBankCriteria criteria, int page, int size)
        {
            var data = CatBankService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// get the list of currencies
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("getAllByQuery")]
        public IActionResult Get(CatBankCriteria criteria)
        {
            var data = CatBankService.Query(criteria);
            return Ok(data);
        }

        /// <summary>
        /// add new currency
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("add")]
        //[Authorize]
        public IActionResult Post(CatBankModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = CatBankService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        
        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">model to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("update")]
        //[Authorize]
        public IActionResult Put(CatBankModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model.Id.ToString(), model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = CatBankService.Update(model);
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
        /// <param name="id">id of data that want to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        //[Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = CatBankService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }


        private string CheckExist(string id, CatBankModel model)
        {
            string message = string.Empty;
            if (id == string.Empty)
            {
                if (CatBankService.Any(x => x.Code.ToString().ToLower() == model.Code.ToString().ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
                }
            }
            else
            {
                if (CatBankService.Any(x => x.BankNameVn.ToLower() == model.BankNameVn.ToLower() && x.Id.ToString().ToLower() != id.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_NAME_EXISTED].Value;
                }
            }
            return message;
        }

    }
}