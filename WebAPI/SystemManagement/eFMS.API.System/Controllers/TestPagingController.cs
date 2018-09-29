using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.Models;
using SystemManagement.DL.Services;
using SystemManagementAPI.Models.Employees;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.Helpers.PagingPrams;
using SystemManagement.DL.Helpers.PageList;
// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SystemManagementAPI.API.Controllers
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TestPagingController : Controller
    {
        private readonly ISysEmployeeService _service;
        private readonly IErrorHandler _errorHandler;
        private readonly IUrlHelper urlHelper;
        public TestPagingController(ISysEmployeeService service, IMapper mapper, IErrorHandler errorHandler, IUrlHelper urlHelper)
        {
            _service = service;
            _errorHandler = errorHandler;
            this.urlHelper = urlHelper;
        }
        
        [HttpPost]
        [Route("GetMAllSysEmployeeModel2", Name = "GetMAllSysEmployeeModel2")]
        public IActionResult GetMAllSysEmployeeModel2(PagingParams pagingParams)
        {
            var model = _service.GetMAllSysEmployeeModel(pagingParams);

            Response.Headers.Add("X-Pagination", model.GetHeader().ToJson());

            var outputModel = new SysEmployeeOutputModel
            {
                Paging = model.GetHeader(),
                Links = GetLinks(model),
                Items = model.List,
            };
            return Ok(outputModel);
        }

        private List<LinkInfo> GetLinks(PagedList<SysEmployeeModel> list)
        {
            var links = new List<LinkInfo>();

            if (list.HasPreviousPage)
                links.Add(CreateLink("GetMAllSysEmployeeModel2", list.PreviousPageNumber,
                           list.PageSize, "previousPage", "GET"));
            links.Add(CreateLink("GetMovies", list.PageNumber,
                           list.PageSize, "self", "GET"));
            if (list.HasNextPage)
                links.Add(CreateLink("GetMAllSysEmployeeModel2", list.NextPageNumber,
                           list.PageSize, "nextPage", "GET"));
            if (list.HasLastPage)
                links.Add(CreateLink("GetMAllSysEmployeeModel2", list.LastPage,
                           list.PageSize, "lastPage", "GET"));
            return links;
        }

        private LinkInfo CreateLink(
            string routeName, int pageNumber, int pageSize,
            string rel, string method)
        {
            return new LinkInfo
            {
                Href = urlHelper.Link(routeName,
                            new { PageNumber = pageNumber, PageSize = pageSize }),
                Rel = rel,
                Method = method
            };
        }
        private SysEmployeeModel ToSysEmployeeInfo(SysEmployeeModel model)
        {
            return new SysEmployeeModel
            {
                Id = model.Id,
               
            };
        }
    }
}
