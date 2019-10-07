using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatBranchController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatBranchService catBranchService;
        private readonly IMapper mapper;

        public CatBranchController(IStringLocalizer<LanguageSub> localizer, ICatBranchService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            catBranchService = service;
            mapper = iMapper;
        }
        /// <summary>
        /// get all branch
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListBranch")]
        public IActionResult GetAllBranch()
        {
            var results = catBranchService.GetListBranches();
            return Ok(results);
        }

    }
}