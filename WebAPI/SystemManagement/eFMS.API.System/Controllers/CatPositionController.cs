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
    public class CatPositionController : ControllerBase
    {
        private readonly ICatPositionService _service;
        private readonly IErrorHandler _errorHandler;

        public CatPositionController(ICatPositionService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/CatPositions
        [HttpGet]
        public List<CatPositionModel> GetCatPosition()
        {

            return _service.Get().ToList();
        }


        // GET: api/CatPositions/5
        [HttpGet("{id}")]
        public IActionResult GetCatPosition([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatPosition = _service.First(i => i.Id == id);

            if (CatPosition == null)
            {
                return NotFound();
            }

            return Ok(CatPosition);
        }

        // PUT: api/CatPositions/5
        [HttpPut("{id}")]
        public IActionResult PutCatPosition([FromRoute] string id, [FromBody] CatPositionModel CatPosition)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != CatPosition.Id)
            {
                return BadRequest();
            }



            try
            {
                _service.Update(CatPosition, i => i.Id == id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CatPositionExists(id))
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

        // POST: api/CatPositions
        [HttpPost]
        public IActionResult PostCatPosition([FromBody] CatPositionModel CatPosition)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                _service.AddAsync(CatPosition);





            }
            catch (DbUpdateException)
            {
                if (CatPositionExists(CatPosition.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCatPosition", new { id = CatPosition.Id }, CatPosition);
        }

        // DELETE: api/CatPositions/5
        [HttpDelete("{id}")]
        public IActionResult DeleteCatPosition([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatPosition = _service.First(i => i.Id == id);
            if (CatPosition == null)
            {
                return NotFound();
            }

            _service.Delete(i => i.Id == id);


            return Ok(CatPosition);
        }

        private bool CatPositionExists(string id)
        {
            return _service.Get(i => i.Id == id).Count() > 0 ? true : false;
        }
    }
}
