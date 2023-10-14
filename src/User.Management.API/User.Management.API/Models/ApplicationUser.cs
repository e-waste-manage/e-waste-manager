using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace User.Management.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name {get;set;}

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

    }
}
