using Business.Abstract;
using Core.Utilities.Query;
using Entities.Concrete;
using Entities.Dtos.shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopsController : ControllerBase
    {
        private readonly IShopService _service;
        public ShopsController(IShopService service) { _service = service; }

        [HttpPost]
        [Authorize]
        public IActionResult Add([FromBody] ShopCreateDto dto)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized("کاربر نامعتبر است.");

            var r = _service.Add(dto, userId);
            return r.Success ? Ok(r.Message) : BadRequest(r.Message);
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public IActionResult Update(Guid id, [FromBody] Shop dto)
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

        //  ✅ frontend: shopService.get(id)
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public IActionResult GetById(Guid id)
        {
            var r = _service.GetById(id);
            if (!r.Success)
                return NotFound(r.Message ?? "Kayıt bulunamadı.");

            return Ok(r.Data);
        }

        [HttpGet("with-shares")]
        // [Authorize]
        public async Task<IActionResult> GetWithShares([FromQuery] Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                return BadRequest("ownerId is required.");

            var result = await _service.GetShopsWithSharesAsync(ownerId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("franchise-owners/{ownerId:guid}/shops")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<FranchiseOwnerShopsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetFranchiseOwnerShops(Guid ownerId)
        {
            if (ownerId == Guid.Empty)
                return BadRequest("ownerId is required.");

            var result = await _service.GetFranchiseOwnerShops(ownerId);

            if (!result.Success)
                return BadRequest(result.Message ?? "İşlem gerçekleştirilemedi.");

            return Ok(result.Data ?? new List<FranchiseOwnerShopsDto>());
        }

        [HttpGet("available-for-contracts")]
        [ProducesResponseType(typeof(List<ShopPickerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAvailableForContracts()
        {
            var r = await _service.GetAvailableShopsAsync();
            return r.Success ? Ok(r.Data) : BadRequest(r.Message);
        }

        // ✅ GetShopListView – artık DataResult içinden sadece Data dönüyor
        [HttpGet("list-view"), AllowAnonymous]
        public IActionResult GetShopListView()
        {
            var result = _service.GetShopListView();
            return result.Success ? Ok(result.Data) : BadRequest(result.Message);
        }

        /////////////////////////////////////////////
        ///

        [HttpGet("assign-screen/mine")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMyShopsForAssignScreen([FromQuery] Guid ownerId, [FromQuery] QueryOptions opt)
        {
            if (ownerId == Guid.Empty)
                return BadRequest("ownerId is required.");

            var result = await _service.GetMyShopsPagedAsync(ownerId, opt);
            return result.Success ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("assign-screen/available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableShopsForAssignScreen([FromQuery] Guid ownerId, [FromQuery] QueryOptions opt)
        {
            if (ownerId == Guid.Empty)
                return BadRequest("ownerId is required.");

            var result = await _service.GetAvailableShopsPagedAsync(ownerId, opt);
            return result.Success ? Ok(result.Data) : BadRequest(result.Message);
        }

    }
}
