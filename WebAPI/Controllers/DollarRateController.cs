using Business.Abstract;
using Entities.Concrete;
using Entities.Dtos.DollarRate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DollarRateController : ControllerBase
    {
        private readonly IDollarRateService _service;

        public DollarRateController(IDollarRateService service)
        {
            _service = service;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll()
        {
            var r = _service.GetAll();
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:int}"), AllowAnonymous]
        public IActionResult Get(int id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("latest"), AllowAnonymous]
        public IActionResult GetLatest()
        {
            var r = _service.GetLatest();
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] DollarRateCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:int}"), Authorize]
        public IActionResult Update(int id, [FromBody] DollarRateUpdateDto dto)
        {
            if (id != dto.Id) return BadRequest("شناسه نامعتبر است.");
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Update(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpDelete("{id:int}"), Authorize]
        public IActionResult Delete(int id)
        {
            var r = _service.Delete(id);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
