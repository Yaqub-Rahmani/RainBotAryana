using Business.Abstract;
using Core.Utilities.Paging;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistrictsController : ControllerBase
    {
        private readonly IDistrictService _service;
        public DistrictsController(IDistrictService service) { _service = service; }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll() => Ok(_service.GetAll().Data);

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public IActionResult Get(int id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] int? provinceId)
        {
            var r = _service.GetDetails(opt, provinceId);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:int}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(int id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] District dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public IActionResult Update(int id, [FromBody] District dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var r = _service.Update(dto);
            return r.Success ? Ok() : BadRequest(r.Message);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public IActionResult Delete(int id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : NotFound(r.Message);
        }
    }
}
