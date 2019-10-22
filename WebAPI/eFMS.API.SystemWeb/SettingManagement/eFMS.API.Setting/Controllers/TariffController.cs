using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Infrastructure.Common;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TariffController : ControllerBase
    {
        private readonly ITariffService tariffService;
        private readonly IStringLocalizer stringLocalizer;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public TariffController(IStringLocalizer<LanguageSub> localizer, ITariffService service)
        {
            stringLocalizer = localizer;
            tariffService = service;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Get(TariffCriteria criteria)
        {
            var results = tariffService.Query(criteria);
            return Ok(results);
        }

        /// <summary>
        /// get and paging the list of tariff
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="page">page to retrieve data</param>
        /// <param name="size">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        //[Authorize]
        public IActionResult Paging(TariffCriteria criteria, int page, int size)
        {
            var data = tariffService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var data = tariffService.Get();
            return Ok(data);
        }

        /// <summary>
        /// Add tariff and list tariff details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddTariff(TariffModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkData = tariffService.CheckExistsDataTariff(model);
            if (!checkData.Success) return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });

            var hs = tariffService.AddTariff(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        /// <summary>
        /// Update tariff and list tariff details
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult UpdateTariff(TariffModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var checkData = tariffService.CheckExistsDataTariff(model);
            if (!checkData.Success) return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });

            var hs = tariffService.UpdateTariff(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        /// <summary>
        /// Delete tariff and list tariff details
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            //Check exists tariff & status tariff
            var checkStatus = tariffService.Get(x => x.Id == id).FirstOrDefault();
            if(checkStatus == null)
            {
                return Ok(new ResultHandle { Status = false, Message = "Not found tariff" });
            }
            else
            {
                if (checkStatus.Status == true)
                {
                    return Ok(new ResultHandle { Status = false, Message = "Not allowed delete tariff" });
                }
            }

            var hs = tariffService.DeleteTariff(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }

        /// <summary>
        /// Get tariff and list tariff detail by tariff id
        /// </summary>
        /// <param name="tariffId"></param>
        /// <returns></returns>
        [HttpGet("GetTariff")]
        public IActionResult GetTariff(Guid tariffId)
        {
            var result = new TariffModel();
            result.setTariff = tariffService.GetTariffById(tariffId);
            result.setTariffDetails = tariffService.GetListTariffDetailByTariffId(tariffId).ToList();
            if (result == null)
            {
                return Ok(new ResultHandle { Status = false, Message = "Không tìm thấy Tariff", Data = result });
            }
            else
            {
                return Ok(new ResultHandle { Status = true, Message = "Success", Data = result });
            }
        }

        /// <summary>
        /// Check duplicate tariff
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CheckDuplicateTariff")]
        public IActionResult CheckDuplicateTariff(SetTariffModel model)
        {
            var checkData = tariffService.CheckDuplicateTariff(model);
            return Ok(new ResultHandle { Status = checkData.Success, Message = checkData.Exception.Message.ToString(), Data = checkData.Code });
        }

    }
}