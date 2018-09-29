using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SystemManagement.DL.Services;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.Models;
using Microsoft.EntityFrameworkCore;

namespace SystemManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class SysAuthorizationDetailController : ControllerBase
    {
        private readonly ISysAuthorizationDetailService _service;
        private readonly IErrorHandler _errorHandler;

        public SysAuthorizationDetailController(ISysAuthorizationDetailService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/SysAuthorizationDetails
        [HttpGet]
        public List<SysAuthorizationDetailModel> GetSysAuthorizationDetail()
        {

            return _service.Get().ToList();
        }

        // GET: api/SysAuthorizationDetails/5
        [HttpGet("{id}")]
        public IActionResult GetSysAuthorizationDetail([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysAuthorizationDetail = _service.First(i => i.Id == id);

            if (SysAuthorizationDetail == null)
            {
                return NotFound();
            }

            return Ok(SysAuthorizationDetail);
        }
        [HttpGet]
        [Route("GetSysAuthorizationDetailBy/{AuthorizationId}")]
        public IActionResult GetSysAuthorizationDetailBy([FromRoute] int AuthorizationId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysAuthorizationDetail = _service.Get(i => i.AuthorizationId== AuthorizationId);

            if (SysAuthorizationDetail == null)
            {
                return NotFound();
            }

            return Ok(SysAuthorizationDetail);
        }
        // PUT: api/SysAuthorizationDetails/5
        [HttpPut("{id}")]
        public IActionResult PutSysAuthorizationDetail([FromRoute] int id, [FromBody] SysAuthorizationDetailModel SysAuthorizationDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != SysAuthorizationDetail.Id)
            {
                return BadRequest();
            }



            try
            {
                var hs =_service.Update(SysAuthorizationDetail, i => i.Id == id);
                if (hs.Success)
                {
                    return Ok(hs);

                }
                else
                {
                    return BadRequest(hs);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SysAuthorizationDetailExists(id))
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

        // POST: api/SysAuthorizationDetails
        [HttpPost]
        public IActionResult PostSysAuthorizationDetail([FromBody] SysAuthorizationDetailModel SysAuthorizationDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var hs= _service.Add(SysAuthorizationDetail);
                if (hs.Success)
                {
                    return Ok(hs);
                }
                else { return BadRequest(hs);
                }
            }
            catch (DbUpdateException)
            {
                if (SysAuthorizationDetailExists(SysAuthorizationDetail.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSysAuthorizationDetail", new { id = SysAuthorizationDetail.Id }, SysAuthorizationDetail);
        }

        // DELETE: api/SysAuthorizationDetails/5
        [HttpDelete("{id}")]
        public IActionResult DeleteSysAuthorizationDetail([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysAuthorizationDetail = _service.First(i => i.Id == id);
            if (SysAuthorizationDetail == null)
            {
                return NotFound();
            }
            var hs =_service.Delete(i => i.Id == id);


            return Ok(hs);
        }

        private bool SysAuthorizationDetailExists(int id)
        {
            return _service.Get(i => i.Id == id).Count() > 0 ? true : false;
        }
    }
}
