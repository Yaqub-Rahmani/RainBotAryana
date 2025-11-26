using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _service;
        public AddressesController(IAddressService service) { _service = service; }

        // --- Okumalar ---
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll().Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt)
        {
            var r = _service.GetDetails(opt);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("owner/{ownerType:int}/{ownerId:guid}")]
        [AllowAnonymous]
        public IActionResult GetByOwner(int ownerType, Guid ownerId)
        {
            var r = _service.GetByOwner((MediaOwnerType)ownerType, ownerId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // --- Yazmalar ---
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] Address dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] Address dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/set-primary")]
        [Authorize]
        public IActionResult SetPrimary(Guid id)
        {
            var r = _service.SetPrimary(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
