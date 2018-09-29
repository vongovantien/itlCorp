using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.Models;
using SystemManagement.DL.Services;

namespace SystemManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class SysAuthorizationController : ControllerBase
    {
        private readonly ISysAuthorizationService _service;
        private readonly IErrorHandler _errorHandler;

        public SysAuthorizationController(ISysAuthorizationService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/SysAuthorizations
        [HttpGet]
        public List<SysAuthorizationModel> GetSysAuthorization()
        {

            return _service.Get().ToList();
        }
        
        //[HttpPost]
        //[Route("search")]
        //public IQueryable<SysAuthorizationModel> search(SysAuthorizationModel[] sysAuthorizationModels)
        //{

        //    return _service.Search(sysAuthorizationModels);
        //}

        [HttpGet("{id}")]
        public IActionResult GetSysAuthorization([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysAuthorization = _service.First(i => i.Id == id);

            if (SysAuthorization == null)
            {
                return NotFound();
            }

            return Ok(SysAuthorization);
        }

        // PUT: api/SysAuthorizations/5
        [HttpPut("{id}")]
        public IActionResult PutSysAuthorization([FromRoute] int id, [FromBody] SysAuthorizationModel SysAuthorization)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != SysAuthorization.Id)
            {
                return BadRequest();
            }



            try
            {
                return Ok( _service.Update(SysAuthorization, i => i.Id == id));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SysAuthorizationExists(id))
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

        // POST: api/SysAuthorizations
        [HttpPost]
        public async Task<IActionResult> PostSysAuthorization([FromBody] SysAuthorizationModel SysAuthorization)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                return Ok(await _service.AddAsync(SysAuthorization));
            }
            catch (DbUpdateException)
            {
                if (SysAuthorizationExists(SysAuthorization.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSysAuthorization", new { id = SysAuthorization.Id }, SysAuthorization);
        }

        // DELETE: api/SysAuthorizations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSysAuthorization([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysAuthorization = _service.First(i => i.Id == id);
            if (SysAuthorization == null)
            {
                return NotFound();
            }
           


            return Ok(await _service.DeleteAsync(i => i.Id == id));
        }

        private bool SysAuthorizationExists(int id)
        {
            return _service.Get(i => i.Id == id).Count() > 0 ? true : false;
        }

    }
}
