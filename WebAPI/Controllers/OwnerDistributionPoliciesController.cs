using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerDistributionPoliciesController : ControllerBase
    {
        private readonly IOwnerDistributionPolicyService _service;
        public OwnerDistributionPoliciesController(IOwnerDistributionPolicyService service) { _service = service; }

        // Reads
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

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("active")]
        [AllowAnonymous]
        public IActionResult GetActive([FromQuery] DateOnly on)
        {
            var r = _service.GetActiveForDate(on);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Writes
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] OwnerDistributionPolicy dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] OwnerDistributionPolicy dto)
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

        // Simulate
        [HttpGet("simulate")]
        [AllowAnonymous]
        public IActionResult Simulate([FromQuery] decimal ownerAmount, [FromQuery] DateOnly? on)
        {
            var r = _service.Simulate(ownerAmount, on);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }
    }
}
