using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.OwnerDistributionPolicy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerDistributionPolicyController : ControllerBase
    {
        private readonly IOwnerDistributionPolicyService _service;

        public OwnerDistributionPolicyController(IOwnerDistributionPolicyService service)
        {
            _service = service;
        }

        [HttpGet, Authorize]
        public IActionResult GetAll()
        {
            var r = _service.GetAll();
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}"), Authorize]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("current"), AllowAnonymous]
        public IActionResult GetCurrent()
        {
            var r = _service.GetCurrent();
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost]
        public IActionResult Add([FromBody] OwnerDistributionPolicyCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] OwnerDistributionPolicyUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("شناسه با مدل هم‌خوانی ندارد.");

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
