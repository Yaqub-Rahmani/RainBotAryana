using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FloorsController : ControllerBase
    {
        private readonly IFloorService _service;
        public FloorsController(IFloorService service) { _service = service; }

        // Okumalar
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? marketId, [FromQuery] bool? status)
            => Ok(_service.GetAll(marketId, status).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? marketId, [FromQuery] bool? status)
        {
            var r = _service.GetDetails(opt, marketId, status);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Yazmalar
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] Entities.Concrete.Floor dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] Entities.Concrete.Floor dto)
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

        [HttpPost("{id:guid}/activate")]
        [Authorize]
        public IActionResult Activate(Guid id)
        {
            var r = _service.Activate(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPost("{id:guid}/deactivate")]
        [Authorize]
        public IActionResult Deactivate(Guid id)
        {
            var r = _service.Deactivate(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
