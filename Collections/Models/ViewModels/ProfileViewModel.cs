using System.ComponentModel.DataAnnotations;

namespace Collections.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        public string Name { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email address")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

#nullable enable
        public IFormFile? Image { get; set; }
#nullable disable
    }
}
