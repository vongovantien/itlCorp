using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.Models;
using SystemManagement.DL.Models.Views;
using SystemManagement.DL.Services;
using SystemManagementAPI.Models.Emploees;
using SystemManagementAPI.Service.Models;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SystemManagementAPI.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly ISysEmployeeService _service;
        private readonly IErrorHandler _errorHandler;

        public EmployeeController(ISysEmployeeService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }
        // GET api/employee
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet(Name = nameof(Get))]
        public async Task<IActionResult> Get()
        {
            IEnumerable<SysEmployeeModel> iSysEmp = await _service.GetAsync();
            if (iSysEmp == null)
                return new NotFoundResult();

            return new OkObjectResult(iSysEmp);
        }

        [HttpGet]
        [Route("GetViewEmployees")]
        public List<vw_sysEmployee> GetViewEmployees()
        {
            return _service.GetViewEmployees();
        }

        // GET api/employeeByID
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet]
        [Route("GetByID/{id}")]
        public IActionResult GetByID(string id)
        {
            SysEmployeeModel sysEmp = _service.First(t => t.Id == id);
            if (sysEmp == null)
                return new NotFoundResult();

            return new OkObjectResult(sysEmp);

        }
        [HttpGet]
        [Route("GetFollowWorkPlace/{workPlaceId}")]
        public List<SysEmployeeModel> GetFollowWorkPlace([Required]Guid workPlaceId)
        {
            return _service.GetFollowWorkPlace(workPlaceId).ToList();
        }

        [HttpGet]
        [Route("GenerateID/{workPlaceId}")]
        public object GenerateID([Required]Guid workPlaceId)
        {
            return _service.GenerateID(workPlaceId);
        }

        [HttpGet]
        [Route("where/criterias/{criteriasString}")]
        public List<SysEmployeeModel> Where(string criteriasString)
        {
            var criteriasModel = JsonConvert.DeserializeObject<WhereRequestModel>(criteriasString);
            var whereResult = _service.Get();
            whereResult = criteriasModel.Criterias.Aggregate(whereResult, (current, attribute)
                => current.Where(entity => (string)entity.GetType().GetProperty(attribute.Key).GetValue(entity, null) == attribute.Value));
            return whereResult.ToList();
        }
        // POST api/employee
        [HttpPost]
        public HandleState Add(SysEmployeeModel entity)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpRequestException(ModelState.Values.First().Errors.First().ErrorMessage);
            }
            return _service.Add(entity);
        }

        // POST api/employee
        [HttpPut]
        public HandleState Update([FromBody]SysEmployeeModel entity)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpRequestException(string.Format(_errorHandler.GetMessage(ErrorMessagesEnum.ModelValidation), "", ModelState.Values.First().Errors.First().ErrorMessage));
            }

            return _service.Update(entity, t => t.Id == entity.Id);
        }

        // DELETE api/employee/5
        [HttpDelete("{id}")]
        public HandleState Delete([Required]string id)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpRequestException(string.Format(_errorHandler.GetMessage(ErrorMessagesEnum.ModelValidation), "", ModelState.Values.First().Errors.First().ErrorMessage));
            }
            return _service.Delete(t => t.Id == id);
            //if (handleState.Success==false)
            //{
            //    string msg = handleState.Exception.ToString();

            //}

        }
    }
}
