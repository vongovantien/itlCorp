using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemManagement.DL.Services;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using AutoMapper;
using SystemManagement.DL.Models;

namespace SystemManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class SysTemplatesController : ControllerBase
    {
        private readonly ISysTemplateService _service;
        private readonly IErrorHandler _errorHandler;

        public SysTemplatesController(ISysTemplateService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/SysTemplates
        [HttpGet]
        public List<SysTemplateModel> GetSysTemplate()
        {
            
            return _service.Get().ToList();
        }

        // GET: api/SysTemplates/5
        [HttpGet("{id}")]
        public IActionResult GetSysTemplate([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sysTemplate =  _service.First(i=>i.Id==id);

            if (sysTemplate == null)
            {
                return NotFound();
            }

            return Ok(sysTemplate);
        }

        // PUT: api/SysTemplates/5
        [HttpPut("{id}")]
        public IActionResult PutSysTemplate([FromRoute] string id, [FromBody] SysTemplateModel sysTemplate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != sysTemplate.Id.Trim())
            {
                return BadRequest();
            }

            

            try
            {
                sysTemplate.Id = _service.First(i => i.Id == id).Id;
                return Ok(_service.Update(sysTemplate,i=>i.Id==id));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SysTemplateExists(id))
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

        // POST: api/SysTemplates
        [HttpPost]
        public async Task<IActionResult> PostSysTemplate([FromBody] SysTemplateModel sysTemplate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                

                await _service.AddAsync(sysTemplate);
                 _service.generateTemplateDetail(sysTemplate.Id);
            }
            catch (DbUpdateException)
            {
                if (SysTemplateExists(sysTemplate.Id))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSysTemplate", new { id = sysTemplate.Id }, sysTemplate);
        }

        // DELETE: api/SysTemplates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSysTemplate([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sysTemplate =  _service.First(i=>i.Id==id);
            if (sysTemplate == null)
            {
                return NotFound();
            }

            if (_service.deleteDetail(sysTemplate)) {
                return Ok(await _service.DeleteAsync(i => i.Id == id));
            };
            return BadRequest(ModelState);
        }

        private bool SysTemplateExists(string id)
        {
            return _service.Get(i=>i.Id==id).Count()>0?true :false;
        }
    }
}