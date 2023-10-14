using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using User.Management.API.Models;
using User.Management.API.Models.Authentication.SignUp;

namespace User.Management.API.Controllers
{
    [Authorize(Roles ="Admin")]
    [Route("api/[controller]")]
    [ApiController]   
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _dbcontext;

        public AdminController(UserManager<ApplicationUser> _context)
        {
            _dbcontext = _context;
        }


        [HttpGet("GetUserDetailsByID")]
        public async Task<IActionResult> GetUserDetailsByID(string UserID)
        {
            var userExist = await _dbcontext.FindByIdAsync(UserID);

            if (userExist == null)
            {
                return NotFound();
            }
            var UserDetails = _dbcontext.Users.Select(t => new
            {
                t.Id, t.Name, t.Email,t.PhoneNumber, t.Address
            }
            ).Where(t => t.Id == UserID).FirstOrDefault();
            return Ok(UserDetails);

        }
    }
}


