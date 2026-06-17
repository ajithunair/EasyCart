using System.ComponentModel.DataAnnotations;

namespace EasyCart.OrderApi.DTOs
{
    public record AppUserDto
    (
        int Id,
        [Required] string Name,
        [Required] string Email,
        [Required] string Address,
        [Required] string PhoneNumber,
        [Required] string Password,
        [Required] string Role
    );
}
