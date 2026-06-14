using EasyCart.ProductApi.DTOs;
using EasyCart.ProductApi.DTOs.Conversions;
using EasyCart.ProductApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EasyCart.ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProduct productInterface) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await productInterface.GetAllAsync();
            if(products == null || !products.Any())
            {
                return NotFound("No products found.");
            }
            return Ok(products.ToDTOs());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var product = await productInterface.FindByIdAsync(id);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            return Ok(product.ToDTO());
        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateProduct([FromBody] ProductDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await productInterface.CreateAsync(productDTO.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateProduct([FromBody] ProductDTO productDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = await productInterface.UpdateAsync(productDTO.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete]
        public async Task<ActionResult<Response>> DeleteProduct(ProductDTO productDTO)
        {
            var response = await productInterface.DeleteAsync(productDTO.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
