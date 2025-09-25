using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoriesController(ICategoryService service) { _service = service; }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAll()
        {
            var result = _service.GetAll();
            return result.Success ? Ok(result.Data) : BadRequest(result.Message);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public IActionResult Get(int id)
        {
            var result = _service.GetById(id);
            return result.Success ? Ok(result.Data) : NotFound(result.Message);
        }

        [HttpPost]
        [Authorize] // istersen kaldır
        public IActionResult Add([FromBody] Category dto)
        {
            var result = _service.Add(dto);
            return result.Success
                ? CreatedAtAction(nameof(Get), new { id = dto.Id }, dto)
                : BadRequest(result.Message);
        }

        [HttpPut("{id:int}")]
        [Authorize] // istersen kaldır
        public IActionResult Update(int id, [FromBody] Category dto)
        {
            if (id != dto.Id) return BadRequest("Id mismatch.");
            var result = _service.Update(dto);
            return result.Success ? Ok() : BadRequest(result.Message);
        }

        [HttpDelete("{id:int}")]
        [Authorize] // istersen kaldır
        public IActionResult Delete(int id)
        {
            var result = _service.Delete(id);
            return result.Success ? Ok() : NotFound(result.Message);
        }
    }
}
