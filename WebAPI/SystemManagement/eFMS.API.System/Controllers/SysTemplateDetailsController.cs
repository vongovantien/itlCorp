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
    public class sysTemplateDetailsController : ControllerBase
    {
        private readonly ISysTemplateDetailService _service;
        private readonly IErrorHandler _errorHandler;

        public sysTemplateDetailsController(ISysTemplateDetailService service, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
        }

        // GET: api/SysTemplateDetailDetails
        [HttpGet]
        public List<SysTemplateDetailModel> GetSysTemplateDetail()
        {

            return _service.Get().OrderBy(x=>x.Stt).ToList();
        }

        // GET: api/SysTemplateDetails/5
        [HttpGet("{id}")]
        public IActionResult GetSysTemplateDetail([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysTemplateDetail = _service.Get(i => i.Templateid == id).OrderBy(x => x.Stt).ToList();

            if (SysTemplateDetail == null)
            {
                return NotFound();
            }

            return Ok(SysTemplateDetail);
        }

        // PUT: api/SysTemplateDetails/5
        [HttpPut("{id}")]
        public IActionResult PutSysTemplateDetail([FromRoute] string id, [FromBody] SysTemplateDetailModel SysTemplateDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != SysTemplateDetail.Templateid.Trim())
            {
                return BadRequest();
            }

            

            try
            {
                SysTemplateDetailModel tmp = _service.First(i => i.Templateid.Equals(id) && i.OrdinalPosition == SysTemplateDetail.OrdinalPosition);
                SysTemplateDetail.Templateid = tmp.Templateid;
                SysTemplateDetail.OrdinalPosition = tmp.OrdinalPosition;
                return Ok(_service.Update(SysTemplateDetail, i => i.Templateid == id && i.OrdinalPosition==SysTemplateDetail.OrdinalPosition));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SysTemplateDetailExists(id, SysTemplateDetail.OrdinalPosition))
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

        // POST: api/SysTemplateDetails
        [HttpPost]
        public IActionResult PostSysTemplateDetail([FromBody] SysTemplateDetailModel SysTemplateDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                SysTemplateDetail.OrdinalPosition =(int) SysTemplateDetail.Stt;
                return Ok( _service.Add(SysTemplateDetail));

                
            }
            catch (DbUpdateException)
            {
                if (SysTemplateDetailExists(SysTemplateDetail.Templateid,SysTemplateDetail.OrdinalPosition))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }
            
        }

        // DELETE: api/SysTemplateDetails/5
        [HttpDelete("{templateid}/{id}")]
        public IActionResult DeleteSysTemplateDetail([FromRoute] string templateid,int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysTemplateDetail = _service.First(i => i.Templateid == templateid && i.OrdinalPosition==id);
            if (SysTemplateDetail == null)
            {
                return NotFound();
            }
            _service.Delete(i => i.Templateid == templateid && i.OrdinalPosition == id);


            return Ok(SysTemplateDetail);
        }

        private bool SysTemplateDetailExists(string id,int o)
        {
            return _service.Get(i => i.Templateid == id && i.OrdinalPosition==o).Count() > 0 ? true : false;
        }
    }
}