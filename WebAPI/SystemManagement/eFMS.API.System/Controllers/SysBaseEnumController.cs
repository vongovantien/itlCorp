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
using SystemManagement.DL.Models.Views;
using SystemManagement.DL.Services;
using SystemManagementAPI.Service.Models;

namespace SystemManagementAPI.Controllers
{
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class SysBaseEnumController : ControllerBase
    {
        private readonly ISysBaseEnumService _service;
        private readonly ISysBaseEnumDetailService _serviceDetail;
        private readonly IErrorHandler _errorHandler;
        private IMapper _mapper;

        public SysBaseEnumController(ISysBaseEnumService service, ISysBaseEnumDetailService serviceDetail, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
            _serviceDetail = serviceDetail;
            _mapper = mapper;
        }

        // GET: api/SysBaseEnums
        [HttpGet]
        public List<SysBaseEnumModel> GetSysBaseEnum()
        {

            return _service.GetSysBaseEnums().ToList();
        }
        [HttpGet]
        [Route("getBaseEnum")]
        public List<sp_getBaseEnum> getBaseEnum(string BaseEnum)
        {
            return _service.getBaseEnum(BaseEnum);
        }
        // GET: api/SysBaseEnums/5
        [HttpGet("{id}")]
        public IActionResult GetSysBaseEnum([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysBaseEnum = _service.getFirst(id);
            
            

            if (SysBaseEnum == null)
            {
                return NotFound();
            }

            return Ok(SysBaseEnum);
        }
        
        // PUT: api/SysBaseEnums/5
        [HttpPut("{id}")]
        public IActionResult PutSysBaseEnum([FromRoute] string id, [FromBody] SysBaseEnumModel SysBaseEnum)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != SysBaseEnum.Key)
            {
                return BadRequest();
            }
            SysBaseEnumModel sysBaseEnumModel = _service.First(i => i.Key == id);
            if(sysBaseEnumModel is null) return NotFound();

            try
            {
                SysBaseEnum.Key = sysBaseEnumModel.Key;
                return Ok( _service.PutSysBaseEnums(SysBaseEnum, i => i.Key == id));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SysBaseEnumExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

           // return NoContent();
        }

        // POST: api/SysBaseEnums
        
        [HttpPost]
        public async Task<IActionResult> PostSysBaseEnum([FromBody] SysBaseEnumModel SysBaseEnum)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _service.PostSysBaseEnums(SysBaseEnum);
            }
            catch (DbUpdateException)
            {
                if (SysBaseEnumExists(SysBaseEnum.Key))
                {
                    return new StatusCodeResult(StatusCodes.Status409Conflict);
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetSysBaseEnum", new { id = SysBaseEnum.Key }, SysBaseEnum);
        }
        // DELETE: api/SysBaseEnums/5
        [HttpDelete("{id}")]
        public IActionResult DeleteSysBaseEnum([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var SysBaseEnum = _service.First(i => i.Key == id);
            if (SysBaseEnum == null)
            {
                return NotFound();
            }
            _service.Delete(i => i.Key == id);


            return Ok(SysBaseEnum);
        }

        private bool SysBaseEnumExists(string id)
        {
            return _service.Get(i => i.Key == id).Count() > 0 ? true : false;
        }
        //[HttpGet]
        //[Route("baseenum")]
        //public List<vw_datatype> baseEnum([FromQuery] string baseEnum)
        //{
        //    List<vw_datatype> vw_Datatype = new List<vw_datatype>();
        //    if (baseEnum == "server")
        //    {
        //        vw_Datatype.Add(new vw_datatype() { id = "SYSTEM", text = "SYSTEM" });
        //        vw_Datatype.Add(new vw_datatype() { id = "CATALOGUGE", text = "CATALOGUGE" });
        //    }
        //    if (baseEnum == "apiVersion")
        //    {
        //        vw_Datatype.Add(new vw_datatype() { id = "v1", text = "v1" });
        //        vw_Datatype.Add(new vw_datatype() { id = "v2", text = "v2" });
        //    }
        //    return vw_Datatype;
        //}
        //[HttpGet]
        //[Route("Datatype")]
        //public List<vw_datatype> vw_Datatypes()
        //{
        //    return _service.vw_Datatypes();
        //}
        //[HttpGet]
        //[Route("District")]
        //public List<vw_catDistrict> vw_catDistrict()
        //{
        //    return _service.vw_catDistrict();
        //}
        //[HttpGet]
        //[Route("CatHub")]
        //public List<vw_catHub> vw_catHub()
        //{
        //    return _service.vw_catHub();
        //}
        //[HttpGet]
        //[Route("Display")]
        //public Dictionary<string, string> Display()
        //{
        //    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
        //    keyValuePairs.Add("Expand", "Expand");
        //    keyValuePairs.Add("id", "ID");
        //    keyValuePairs.Add("Inline", "Inline");
        //    keyValuePairs.Add("Modal", "Modal");
        //    keyValuePairs.Add("text", "Text");
        //    return keyValuePairs;
        //}
        [HttpPost]
        [Route("SysBaseEnumDetail")]
        public IActionResult PostSysBaseEnumDetail([FromBody] SysBaseEnumDetailModel sysBaseEnumDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return Ok(_serviceDetail.Add(sysBaseEnumDetail));

        }
        [HttpPut]
        [Route("SysBaseEnumDetail/{id}")]
        public IActionResult PutSysBaseEnumDetail([FromRoute]string id,[FromBody] SysBaseEnumDetailModel sysBaseEnumDetail)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            SysBaseEnumDetailModel old = _serviceDetail.First(i => i.BaseEnumKey == sysBaseEnumDetail.BaseEnumKey && i.Id == sysBaseEnumDetail.Id);
            sysBaseEnumDetail.BaseEnumKey = old.BaseEnumKey;
            sysBaseEnumDetail.Id = old.Id;
            sysBaseEnumDetail =_mapper.Map(sysBaseEnumDetail,old);
            return Ok(_serviceDetail.Update(sysBaseEnumDetail,i=>i.BaseEnumKey==sysBaseEnumDetail.BaseEnumKey && i.Id==sysBaseEnumDetail.Id));

        }
        [HttpDelete]
        [Route("SysBaseEnumDetail/{BaseEnumKey}/{id}")]
        public IActionResult DeleteSysBaseEnumDetail([FromRoute]string BaseEnumKey, [FromRoute]string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            SysBaseEnumDetailModel old = _serviceDetail.First(i => i.BaseEnumKey == BaseEnumKey && i.Id == id);
            return Ok(_serviceDetail.Delete( i => i.BaseEnumKey == old.BaseEnumKey && i.Id == old.Id));

        }
    }
}
