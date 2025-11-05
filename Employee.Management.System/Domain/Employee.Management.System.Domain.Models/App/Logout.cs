using System.ComponentModel.DataAnnotations;

namespace Employee.Management.System.Domain.Models.App
{
    public class Logout
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? UserLogin { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string UserPassword { get; set; } = string.Empty;
        public long UserId { get; set; }
    }
}
