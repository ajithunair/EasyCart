using System.ComponentModel.DataAnnotations;

namespace EasyCart.AuthApi.DTOs
{
    public record AppUserDto
    (
        int Id,
        [Required] string Name,
        [Required, EmailAddress] string Email,
        [Required] string Address,
        [Required] string PhoneNumber,
        [Required] string Password,
        [Required] string Role
    );
}
