using Microsoft.AspNetCore.Identity;

namespace Collections.Models.ViewModels
{
    public class RoleEditViewModel
    {
        public string UserId { get; set; }
        
        public string UserEmail { get; set; }
        
        public List<IdentityRole> AllRoles { get; set; }
        
        public IList<string> UserRoles { get; set; }
        
        public RoleEditViewModel()
        {
            AllRoles = new List<IdentityRole>();
            UserRoles = new List<string>();
        }
    }
}
