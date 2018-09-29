using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Mvc;
using SystemManagement.DL.Infrastructure.ErrorHandler;
using SystemManagement.DL.IService;
using SystemManagement.DL.Models;
using SystemManagement.DL.Services;
namespace SystemManagementAPI.Controllers
{

    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {

        private readonly ISysMenuService _service;
        private readonly ICatShipmentTypeService _Shipment;
        private readonly IErrorHandler _errorHandler;

        public MenuController(ISysMenuService service, ICatShipmentTypeService Shipment, IMapper mapper, IErrorHandler errorHandler)
        {
            _service = service;
            _errorHandler = errorHandler;
            _Shipment = Shipment;
        }
    
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet(Name = nameof(GetAll))]
        public List<SysMenuModel> GetAll()
        {
            return  _service.Get().ToList();
        }
    
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet]
        [Route("getmenuview")]
        public List<MenuEntity> getmenuview()
        {
            return _service.GetMenuViewModel().ToList();
        }
  
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet]
        [Route("GetByID/{id}")]
        public SysMenuModel GetByID(string id)
        {
            return _service.First(t => t.Id == id);
        }
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Produces("application/json")]
        [HttpGet]
        [Route("GetShipmentType")]
        public List<CatShipmentTypeModel> GetShipmentType()
        {
            return _Shipment.Get().ToList();
        }
        [HttpPost]
        public IActionResult Add([FromBody]SysMenuModel entity)
        {   if ((entity.ForWorkPlace ?? "") == "")
                entity.ForWorkPlace = "Both";
            if ((entity.ParentId ?? "") == "")
                entity.ForServiceType = null;
            HandleState handleState = new HandleState();
            if (!ModelState.IsValid)
            {
                throw new HttpRequestException(ModelState.Values.First().Errors.First().ErrorMessage);
            }

            if (((entity.ParentId ?? "") == "" && (entity.ForServiceType ?? "") == "")|| (entity.ForServiceType ?? "") == "")
            handleState = _service.Add(entity);
            if((entity.ParentId ?? "") != "" && (entity.ForServiceType ?? "") != "")
            {
                SysMenuModel MiddleMenu = _service.First(t => t.Id == entity.ParentId + entity.ForServiceType);
                List<string> Shipment = _Shipment.Get().Select(t => t.Id).ToList();
                List<string> ForServiceType = new List<string>();
                foreach(string ship in Shipment)
                {
                    ForServiceType.Add(entity.ParentId+ship);
                }
                int maxSequency = _service.Get().Where(t => t.ParentId == entity.ParentId && ForServiceType.Contains(t.Id)).Select(t => t.Sequence).Max()??0;
                    if(MiddleMenu==null)
                {
                    SysMenuModel AddMiddleMenu = new SysMenuModel()
                    {
                        ParentId = entity.ParentId,
                        Id = entity.ParentId + entity.ForServiceType,
                        NameVn = entity.ForServiceType,
                        NameEn = entity.ForServiceType,
                        Sequence = maxSequency+1,
                        ForServiceType = entity.ForServiceType
                    };
                    handleState = _service.Add(AddMiddleMenu);
                    if (handleState.Success)
                    {
                        entity.ParentId = AddMiddleMenu.Id;
                        handleState = _service.Add(entity);
                    }
                }
                    else
                {
                    entity.ParentId = entity.ParentId+entity.ForServiceType;
                    handleState = _service.Add(entity);
                }
            }
            if (handleState.Success==false)
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }

        [HttpPut]
        public IActionResult Update([FromBody]SysMenuModel entity)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpRequestException(string.Format(_errorHandler.GetMessage(ErrorMessagesEnum.ModelValidation), "", ModelState.Values.First().Errors.First().ErrorMessage));
            }

            HandleState handleState = _service.Update(entity, t => t.Id == entity.Id);
            if (handleState.Success == false)
            {
                return new NotFoundResult();

            }
            return new OkResult();
        }

     
        [HttpDelete("{id}")]
        public IActionResult Delete([Required]string id)
        {
            if (!ModelState.IsValid)
            {
                throw new HttpRequestException(string.Format(_errorHandler.GetMessage(ErrorMessagesEnum.ModelValidation), "", ModelState.Values.First().Errors.First().ErrorMessage));
            }
            HandleState handleState = _service.Delete(t => t.Id == id);
            if (handleState.Success == false)
            {
                return new NotFoundResult();

            }
            return new OkResult();

        }

    }
}