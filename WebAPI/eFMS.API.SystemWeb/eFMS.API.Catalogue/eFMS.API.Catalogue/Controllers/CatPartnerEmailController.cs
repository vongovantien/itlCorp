using System;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatPartnerEmailController : ControllerBase
    {

        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatPartnerEmailService catPartnerEmailService;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatPartnerService</param>
        /// <param name="hostingEnvironment"></param>
        public CatPartnerEmailController(IStringLocalizer<LanguageSub> localizer,
            ICatPartnerEmailService service,
            IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catPartnerEmailService = service;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get partners email by partner Id
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(string partnerId)
        {
            var results = catPartnerEmailService.GetBy(partnerId);
            return Ok(results);
        }

        /// <summary>
        /// get partners email by partner Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Get(Guid id)
        {
            var data = catPartnerEmailService.GetDetail(id);
            return Ok(data);
        }

        /// <summary>
        /// add email partner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(CatPartnerEmailModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(string.Empty, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var result = catPartnerEmailService.Add(model);
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
        public IActionResult Put(string id, CatPartnerEmailModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(id, model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = catPartnerEmailService.Update(model);
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
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catPartnerEmailService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        private string CheckExist(string id, CatPartnerEmailModel model)
        {
            string message = string.Empty;
            if (id.Length == 0)
            {

                if (catPartnerEmailService.Any(x => x.Type == model.Type && x.OfficeId == model.OfficeId && x.PartnerId == model.PartnerId) )
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }

            }
            else
            {
                if (catPartnerEmailService.Any(x => x.Type == model.Type && x.OfficeId == model.OfficeId && x.PartnerId == model.PartnerId && x.Id.ToString() != id))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            return message;
        }


    }
}