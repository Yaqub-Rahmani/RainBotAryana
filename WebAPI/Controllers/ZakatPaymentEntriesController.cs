using Business.Abstract;
using Entities.Concrete;
using Entities.Dtos.ZakatPayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/zakat-payments")]
    public class ZakatPaymentEntriesController : ControllerBase
    {
        private readonly IZakatPaymentEntryService _service;

        public ZakatPaymentEntriesController(IZakatPaymentEntryService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] ZakatPaymentEntryFilterDto filter)
        {
            return Ok(_service.GetAll(filter).Data);
        }

        [HttpGet("{id}")]
        public IActionResult Detail(Guid id)
        {
            var result = _service.GetDetail(id);
            return result.Success ? Ok(result.Data) : NotFound(result.Message);
        }

        [HttpPost]
        public IActionResult Add(ZakatPaymentEntryCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(_service.Add(dto, userId).Message);
        }

        [HttpPut]
        public IActionResult Update(ZakatPaymentEntryUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(_service.Update(dto, userId).Message);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            return Ok(_service.Delete(id).Message);
        }

        [HttpPost("{id}/approve")]
        public IActionResult Approve(Guid id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            return Ok(_service.Approve(id, userId).Message);
        }
    }

}
