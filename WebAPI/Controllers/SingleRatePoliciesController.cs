using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SingleRatePoliciesController : ControllerBase
    {
        private readonly ISingleRatePolicyService _service;
        public SingleRatePoliciesController(ISingleRatePolicyService service) { _service = service; }

        // Reads
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] SingleRateType? type)
            => Ok(_service.GetAll(type).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] SingleRateType? type)
        {
            var r = _service.GetDetails(opt, type);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public IActionResult GetActive([FromQuery] SingleRateType type, [FromQuery] DateOnly on)
        {
            var r = _service.GetActive(type, on);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Writes
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] SingleRatePolicy dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] SingleRatePolicy dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        // Simulate
        [HttpGet("simulate")]
        [AllowAnonymous]
        public IActionResult Simulate([FromQuery] SingleRateType type, [FromQuery] decimal baseAmount, [FromQuery] DateOnly? on)
        {
            var r = _service.Simulate(type, baseAmount, on);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }
    }
}
