using Business.Abstract;
using Entities.Concrete;
using Entities.Dtos.MoneyEntry;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoneyEntryController : ControllerBase
    {
        private readonly IMoneyEntryService _service;

        public MoneyEntryController(IMoneyEntryService service)
        {
            _service = service;
        }


        [HttpPost, Authorize]
        public IActionResult Add([FromBody] MoneyEntryCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }
    }
}
