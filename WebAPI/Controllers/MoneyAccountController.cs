using Business.Abstract;
using Entities.Dtos.MoneyAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoneyAccountController : ControllerBase
    {
        private readonly IMoneyAccountService _service;

        public MoneyAccountController(IMoneyAccountService service)
        {
            _service = service;
        }

        [HttpGet, AllowAnonymous]
        public IActionResult GetAll([FromQuery] bool includeInactive = false)
        {
            var r = _service.GetAll(includeInactive);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}"), AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpPost, Authorize]
        public IActionResult Add([FromBody] MoneyAccountCreateDto dto)
        {
            var r = _service.Add(dto);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}"), Authorize]
        public IActionResult Update(Guid id, [FromBody] MoneyAccountUpdateDto dto)
        {
            if (id != dto.Id)
                return BadRequest("شناسه با مدل هم‌خوانی ندارد.");

            var r = _service.Update(dto);
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
