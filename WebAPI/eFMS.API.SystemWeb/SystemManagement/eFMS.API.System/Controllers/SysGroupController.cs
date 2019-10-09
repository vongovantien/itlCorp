using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
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
    public class SysGroupController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysGroupService sysGroupService;
        private readonly IMapper mapper;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="groupService"></param>
        /// <param name="imapper"></param>
        public SysGroupController(IStringLocalizer<LanguageSub> localizer, 
            ISysGroupService groupService,
            IMapper imapper) {
            stringLocalizer = localizer;
            sysGroupService = groupService;
            mapper = imapper;
        }

        /// <summary>
        /// get all groups
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = sysGroupService.Get();
            return Ok(results);
        }

        /// <summary>
        /// get detai group by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Get(short id)
        {
            var result = sysGroupService.First(x => x.Id == id);
            return Ok(result);
        }

        /// <summary>
        /// add new group
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public IActionResult Post(SysGroupModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = "admin";
            model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
            var hs = sysGroupService.Add(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
