using System.ComponentModel.DataAnnotations;

namespace EventHub.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}