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
    public class CollectionAllocationController : ControllerBase
    {
        private readonly ICollectionAllocationService _service;

        public CollectionAllocationController(ICollectionAllocationService service)
        {
            _service = service;
        }

        [HttpGet("{id:guid}"), Authorize]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("by-collection/{collectionId:guid}"), Authorize]
        public IActionResult GetByCollection(Guid collectionId)
        {
            var r = _service.GetByCollection(collectionId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] CollectionAllocationCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
