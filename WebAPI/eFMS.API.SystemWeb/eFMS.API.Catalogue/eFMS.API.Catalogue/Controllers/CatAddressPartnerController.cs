using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Catalogue.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatAddressPartnerController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatAddressPartnerService catAddressPartnerService;
        private readonly ICurrentUser currentUser;
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        ///
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="service">inject interface ICatAddressPartnerService</param>
        /// <param name="currUser">inject interface ICurrentUser</param>
        /// <param name="hostingEnvironment">inject interface IHostingEnvironment</param>

        public CatAddressPartnerController(IStringLocalizer<LanguageSub> localizer, ICatAddressPartnerService service,
        ICurrentUser currUser, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            catAddressPartnerService = service;
            currentUser = currUser;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// get the list of all address
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("getAll")]
        public IActionResult Get()
        {
            var data = catAddressPartnerService.GetAll()?.OrderBy(x => x.DatetimeCreated);
            return Ok(data);
        }

        /// <summary>
        /// get address by id
        /// </summary>
        /// <param name="id">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDetailById/{id}")]
        public IActionResult Get(Guid id)
        {
            var data = catAddressPartnerService.GetDetail(id);
            return Ok(data);
        }
        /// <summary>
        /// get address by id
        /// </summary>
        /// <param name="partnerId">id of data that need to retrieve</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddressByPartner/{partnerId}")]
        public IActionResult GetAddressByPartner(Guid partnerId)
        {
            var data = catAddressPartnerService.GetAddressByPartnerId(partnerId);
            return Ok(data);
        }
        /// <summary>
        /// add new item
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("add")]
        [Authorize]
        public IActionResult Post(CatAddressPartnerModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if (model.PartnerId != null)
            {
                var checkExistMessage = CheckExist(string.Empty, model);
                if (checkExistMessage.Length > 0)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
                }
            }
            var hs = catAddressPartnerService.Add(model);
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
        [Authorize]
        public IActionResult Put(CatAddressPartnerModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (model.PartnerId != null)
            {
                var checkExistMessage = CheckExist(model.Id.ToString(), model);
                if (checkExistMessage.Length > 0)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
                }
            }


            var hs = catAddressPartnerService.Update(model);
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
        /// <param name="id">id that want to delete</param>
        /// <returns></returns>
        [HttpDelete("DeleteById/{id}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = catAddressPartnerService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        private string CheckExist(string id, CatAddressPartnerModel model)
        {
            string message = string.Empty;
            if (id == string.Empty)
            {
                if (catAddressPartnerService.Any(x => x.CountryId == model.CountryId && x.CityId.ToString().ToLower() == model.CityId.ToString().ToLower()
                    && x.DistrictId.ToString().ToLower() == model.DistrictId.ToString().ToLower()
                    && x.WardId.ToString().ToLower() == model.WardId.ToString().ToLower()
                    && x.StreetAddress.ToLower().Trim() == model.StreetAddress.ToLower().Trim()
                    && x.PartnerId.ToString().ToLower() == model.PartnerId.ToString().ToLower() 
                    && x.ContactPerson.ToLower().Trim() == model.ContactPerson.ToLower().Trim()
                    && x.Tel.Trim() == model.Tel.Trim() && x.ShortNameAddress.ToLower().Trim() == model.ShortNameAddress.ToLower().Trim()
                    && x.AddressType == model.AddressType))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }
            else
            {
                if (catAddressPartnerService.Any(x => x.CountryId == model.CountryId && x.CityId.ToString().ToLower() == model.CityId.ToString().ToLower()
                    && x.DistrictId.ToString().ToLower() == model.DistrictId.ToString().ToLower()
                    && x.WardId.ToString().ToLower() == model.WardId.ToString().ToLower()
                    && x.StreetAddress.ToLower().Trim() == model.StreetAddress.ToLower().Trim()
                    && x.PartnerId.ToString().ToLower() == model.PartnerId.ToString().ToLower()
                    && x.ContactPerson.ToLower().Trim() == model.ContactPerson.ToLower().Trim()
                    && x.Tel.Trim() == model.Tel.Trim() && x.ShortNameAddress.ToLower().Trim() == model.ShortNameAddress.ToLower().Trim()
                    && x.AddressType == model.AddressType
                    && x.Id.ToString().ToLower() != id.ToLower()))
                {
                    message = stringLocalizer[LanguageSub.MSG_OBJECT_DUPLICATED].Value;
                }
            }    
            return message;
        }
    }
}
