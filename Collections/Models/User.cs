using Microsoft.AspNetCore.Identity;

namespace Collections.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

#nullable enable
        public string? Image { get; set; }
#nullable disable
    }
}
