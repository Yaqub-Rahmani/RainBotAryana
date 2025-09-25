using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerDistributionsController : ControllerBase
    {
        private readonly IOwnerDistributionService _service;
        public OwnerDistributionsController(IOwnerDistributionService service) { _service = service; }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? contractId)
            => Ok(_service.GetAll(contractId).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? contractId)
        {
            var r = _service.GetDetails(opt, contractId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] OwnerDistribution dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] OwnerDistribution dto)
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
    }
}
