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
using SystemManagement.DL.Models;
using SystemManagement.DL.Services;


namespace SystemManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CatPlaceController : ControllerBase
    {
        private readonly ICatPlaceService _service;
        private readonly IErrorHandler _errorHandler;

        public CatPlaceController(ICatPlaceService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/CatPlaces
        [HttpGet]
        public List<CatPlaceModel> GetCatPlace()
        {

            return _service.Get().ToList();
        }

        [HttpGet]
        [Route("getCatPlaceFollowHubAndBranch")]
        public List<CatPlaceModel> GetCatPlaceFollowHubAndBranch()
        {
            return _service.GetCatPlaceFollowHubAndBranch();
        }

        // GET: api/CatPlaces/5
        [HttpGet("{id}")]
        public IActionResult GetCatPlace([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatPlace = _service.First(i => i.Id == id);

            if (CatPlace == null)
            {
                return NotFound();
            }

            return Ok(CatPlace);
        }

        // PUT: api/CatPlaces/5
        [HttpPut("{id}")]
        public IActionResult PutCatPlace([FromRoute] Guid id, [FromBody] CatPlaceModel CatPlace)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != CatPlace.Id)
            {
                return BadRequest();
            }



            try
            {
                _service.Update(CatPlace, i => i.Id == id);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CatPlaceExists(id))
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

        // POST: api/CatPlaces
        [HttpPost]
        public async Task<IActionResult> PostCatPlace([FromBody] CatPlaceModel CatPlace)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                await _service.AddAsync(CatPlace);





            }
            catch (DbUpdateException)
            {
                if (CatPlaceExists(CatPlace.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCatPlace", new { id = CatPlace.Id }, CatPlace);
        }

        // DELETE: api/CatPlaces/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatPlace([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var CatPlace = _service.First(i => i.Id == id);
            if (CatPlace == null)
            {
                return NotFound();
            }

            await _service.DeleteAsync(i => i.Id == id);


            return Ok(CatPlace);
        }

        private bool CatPlaceExists(Guid id)
        {
            return _service.Get(i => i.Id == id).Count() > 0 ? true : false;
        }
    }
}
