using Microsoft.AspNetCore.Identity;

namespace User.Management.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name {get;set;}
        public string Address { get; set; }
    }
}
