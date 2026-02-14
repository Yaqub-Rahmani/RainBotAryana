using Business.Abstract;
using Entities.Concrete;
using Entities.Dtos.PaymentOutflow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentOutflowsController : ControllerBase
    {
        private readonly IPaymentOutflowService _service;
        public PaymentOutflowsController(IPaymentOutflowService service) => _service = service;

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] PaymentOutflowCreateFromUiDto dto, CancellationToken ct)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = await _service.CreateAsync(dto, userId, ct);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }
    }
}
