using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _svc;
        public CategoriesController(ICategoryService svc) { _svc = svc; }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _svc.GetAll();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getbyid/{id:int}")]
        public IActionResult GetById(int id)
        {
            var result = _svc.GetById(id);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] Category category)
        {
            var result = _svc.Add(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] Category category)
        {
            var result = _svc.Update(category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var result = _svc.Delete(id);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
