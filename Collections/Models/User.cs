using Microsoft.AspNetCore.Identity;

namespace Collections.Models
{
    public class User : IdentityUser
    {
        private string _name;
        public string Name
        {
            get => _name; 
            set
            {
                _name = String.Join(' ', 
                        value.Split(' ').Select(n => n[0].ToString().ToUpper() + n.Substring(1).ToLower()).ToArray()
                    );
            }
        }

#nullable enable
        public string? Image { get; set; }
#nullable disable
    }
}
