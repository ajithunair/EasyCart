using System.ComponentModel.DataAnnotations;

namespace EasyCart.AuthApi.DTOs
{
    public record GetUserDto
    (
        int Id,
        [Required] string Name,
        [Required, EmailAddress] string Email,
        [Required] string Address,
        [Required] string PhoneNumber,
        [Required] string Role
    );
}
