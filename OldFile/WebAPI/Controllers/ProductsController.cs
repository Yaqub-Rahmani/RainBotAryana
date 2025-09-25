using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _svc;
        public ProductsController(IProductService svc) { _svc = svc; }

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
        public IActionResult Add([FromBody] Product product)
        {
            var result = _svc.Add(product);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("update")]
        public IActionResult Update([FromBody] Product product)
        {
            var result = _svc.Update(product);
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
