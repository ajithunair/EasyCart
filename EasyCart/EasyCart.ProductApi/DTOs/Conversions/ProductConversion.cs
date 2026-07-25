using EasyCart.ProductApi.Entities;

namespace EasyCart.ProductApi.DTOs.Conversions
{
    public static class ProductConversion
    {
        public static ProductDTO ToDTO(this Product product)
        {
            return new ProductDTO(product.Id, product.Name, product.Quantity, product.Price);
        }

        public static Product ToEntity(this ProductDTO productDTO)
        {
            return new Product
            {
                Id = productDTO.Id,
                Name = productDTO.Name,
                Quantity = productDTO.Quantity,
                Price = productDTO.Price
            };
        }

        public static Product ToEntity(this ProductCreateDTO productDTO)
        {
            return new Product
            {
                Name = productDTO.Name,
                Quantity = productDTO.Quantity,
                Price = productDTO.Price
            };
        }

        public static Product ToEntity(this ProductUpdateDTO productDTO)
        {
            return new Product
            {
                Id = productDTO.Id,
                Name = productDTO.Name,
                Quantity = productDTO.Quantity,
                Price = productDTO.Price
            };
        }

        public static IEnumerable<ProductDTO> ToDTOs(this IEnumerable<Product> products)
        {
            return products.Select(ToDTO);
        }
    }
}
