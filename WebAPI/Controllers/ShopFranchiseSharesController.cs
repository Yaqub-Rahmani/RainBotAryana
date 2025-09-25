using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopFranchiseSharesController : ControllerBase
    {
        private readonly IShopFranchiseShareService _service;
        public ShopFranchiseSharesController(IShopFranchiseShareService service) { _service = service; }

        // Okuma
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll([FromQuery] Guid? shopId, [FromQuery] Guid? franchiseOwnerId)
            => Ok(_service.GetAll(shopId, franchiseOwnerId).Data);

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult Get(Guid id)
        {
            var r = _service.GetById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        [HttpGet("details")]
        [AllowAnonymous]
        public IActionResult GetDetails([FromQuery] QueryOptions opt, [FromQuery] Guid? shopId, [FromQuery] Guid? franchiseOwnerId, [FromQuery] DateOnly? on)
        {
            var r = _service.GetDetails(opt, shopId, franchiseOwnerId, on);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        [HttpGet("{id:guid}/detail")]
        [AllowAnonymous]
        public IActionResult GetDetail(Guid id)
        {
            var r = _service.GetDetailById(id);
            return r.Success ? Ok(r.Data) : NotFound(r.Message);
        }

        // Aktif paylar (tarih bazlı)
        [HttpGet("active/{shopId:guid}")]
        [AllowAnonymous]
        public IActionResult ActiveOn(Guid shopId, [FromQuery] DateOnly on)
            => Ok(_service.GetActiveOn(shopId, on).Data);

        // Özet
        [HttpGet("summary/{shopId:guid}")]
        [AllowAnonymous]
        public IActionResult Summary(Guid shopId, [FromQuery] DateOnly on)
        {
            var r = _service.GetSummary(shopId, on);
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // Yazma
        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] ShopFranchiseShare dto)
        {
            var r = _service.Add(dto);
            return r.Success ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] ShopFranchiseShare dto)
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
