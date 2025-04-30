using System.ComponentModel.DataAnnotations;

namespace Collections.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email address")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Display(Name = "Remember me!")]
        public bool RememberMe { get; set; }
    }
}
