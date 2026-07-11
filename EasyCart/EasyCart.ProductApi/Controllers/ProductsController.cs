using EasyCart.ProductApi.DTOs;
using EasyCart.ProductApi.DTOs.Conversions;
using EasyCart.ProductApi.Entities;
using EasyCart.ProductApi.Interfaces;
using EasyCart.SharedLibrary.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyCart.ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController(IProduct productInterface) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var products = await productInterface.GetAllAsync();
            if (!products.Any())
            {
                return NotFound("No products found.");
            }

            return Ok(products.ToDTOs());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
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
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response>> CreateProduct([FromBody] ProductCreateDTO productDTO)
        {
            var response = await productInterface.CreateAsync(productDTO.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response>> UpdateProduct(int id, [FromBody] ProductUpdateDTO productDTO)
        {
            if (id != productDTO.Id)
            {
                return BadRequest("Route id does not match the payload id.");
            }

            // The route id is the source of truth, so we guard against accidental mismatches early.
            var response = await productInterface.UpdateAsync(productDTO.ToEntity());
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Response>> DeleteProduct(int id)
        {
            // Only the identifier is needed for deletion, so callers do not need a full request body.
            var response = await productInterface.DeleteAsync(new Product { Id = id });
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
