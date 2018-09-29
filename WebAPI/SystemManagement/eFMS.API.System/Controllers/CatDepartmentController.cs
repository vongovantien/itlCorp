using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.IService;
using SystemManagement.DL.Models;
using SystemManagement.DL.Services;

namespace SystemManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CatDepartmentController : ControllerBase
    {
        private readonly ICatDepartmentService _service;
        private readonly IErrorHandler _errorHandler;

        public CatDepartmentController(ICatDepartmentService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/CatDepartments
        [HttpGet]
        public List<CatDepartmentModel> GetCatDepartment()
        {

            return _service.Get().ToList();
        }


        // GET: api/CatDepartments/5
        [HttpGet("{id}")]
        public IActionResult GetCatDepartment([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatDepartment = _service.First(i => i.Id == id);

            if (CatDepartment == null)
            {
                return NotFound();
            }

            return Ok(CatDepartment);
        }

        // PUT: api/CatDepartment/5
        [HttpPut("{id}")]
        public IActionResult PutCatDepartment([FromRoute] string id, [FromBody] CatDepartmentModel catDepartment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != catDepartment.Id)
            {
                return BadRequest();
            }



            try
            {
                _service.Update(catDepartment, i => i.Id == id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CatDepartmentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CatDepartments
        [HttpPost]
        public IActionResult PostCatDepartment([FromBody] CatDepartmentModel catDepartment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                _service.AddAsync(catDepartment);





            }
            catch (DbUpdateException)
            {
                if (CatDepartmentExists(catDepartment.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCatDepartment", new { id = catDepartment.Id }, catDepartment);
        }

        // DELETE: api/CatDepartments/5
        [HttpDelete("{id}")]
        public IActionResult DeleteCatDepartment([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatDepartment = _service.First(i => i.Id == id);
            if (CatDepartment == null)
            {
                return NotFound();
            }

            _service.Delete(i => i.Id == id);


            return Ok(CatDepartment);
        }

        private bool CatDepartmentExists(string id)
        {
            return _service.Get(i => i.Id == id).Count() > 0 ? true : false;
        }
    }
}
