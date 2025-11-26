using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.Collection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionService _service;
        public CollectionsController(ICollectionService service) => _service = service;

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? tenantId, [FromQuery] Guid? shopId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
            => Ok(_service.GetAll(tenantId, shopId, fromUtc, toUtc).Data);

        [HttpGet("{id:guid}"), AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("{id:guid}/detail"), AllowAnonymous]
        public IActionResult Detail(Guid id)
        {
            var r = _service.GetDetail(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] CollectionCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] CollectionUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("شناسه نامعتبر است.");
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Update(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}"), Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
