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
    public class CatSaleResourceController : ControllerBase
    {
        private readonly ICatSaleResourceService _service;
        private readonly IErrorHandler _errorHandler;

        public CatSaleResourceController(ICatSaleResourceService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/CatSaleResources
        [HttpGet]
        public List<CatSaleResourceModel> GetCatSaleResource()
        {

            return _service.Get().ToList();
        }


        // GET: api/CatSaleResources/5
        [HttpGet("{id}")]
        public IActionResult GetCatSaleResource([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatSaleResource = _service.First(i => i.Id == id);

            if (CatSaleResource == null)
            {
                return NotFound();
            }

            return Ok(CatSaleResource);
        }

        // PUT: api/CatSaleResources/5
        [HttpPut("{id}")]
        public IActionResult PutCatSaleResource([FromRoute] string id, [FromBody] CatSaleResourceModel CatSaleResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != CatSaleResource.Id)
            {
                return BadRequest();
            }



            try
            {
                _service.Update(CatSaleResource, i => i.Id == id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CatSaleResourceExists(id))
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

        // POST: api/CatSaleResources
        [HttpPost]
        public IActionResult PostCatSaleResource([FromBody] CatSaleResourceModel CatSaleResource)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                _service.AddAsync(CatSaleResource);





            }
            catch (DbUpdateException)
            {
                if (CatSaleResourceExists(CatSaleResource.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCatSaleResource", new { id = CatSaleResource.Id }, CatSaleResource);
        }

        // DELETE: api/CatSaleResources/5
        [HttpDelete("{id}")]
        public IActionResult DeleteCatSaleResource([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatSaleResource = _service.First(i => i.Id == id);
            if (CatSaleResource == null)
            {
                return NotFound();
            }

            _service.Delete(i => i.Id == id);


            return Ok(CatSaleResource);
        }

        private bool CatSaleResourceExists(string id)
        {
            return _service.Get(i => i.Id == id).Count() > 0 ? true : false;
        }
    }
}
