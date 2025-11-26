using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.CollectionAllocation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionAllocationsController : ControllerBase
    {
        private readonly ICollectionAllocationService _service;

        public CollectionAllocationsController(ICollectionAllocationService service)
        {
            _service = service;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? collectionId, [FromQuery] Guid? contractId)
            => Ok(_service.GetAll(collectionId, contractId).Data);

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
        public IActionResult Add([FromBody] CollectionAllocationCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] CollectionAllocationUpdateDto dto)
        {
            if (dto.Id != id) return BadRequest("شناسه نامعتبر است.");
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
