using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Web_API_Test.DAL.Authentication;
using Web_API_Test.DAL.Exceptions;

namespace Web_API_Test.Service.Repositories
{
    public interface IAuthentication
    {
        Task<JwtSecurityToken> Login(LoginModel loginmodel);
        Task<bool> Register(RegisterModel registerModel);
        Task<bool> AdminRegister(RegisterModel registerModel);
    }

    public class Authenticate : IAuthentication
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public Authenticate(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }
        public async Task<bool> AdminRegister(RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                throw new StatusException { Status = "Error", Message = "User already exists!" };

            IdentityUser user = new IdentityUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new StatusException { Status = "Error", Message = "User creation failed! Please check user details and try again." };

            if (!await roleManager.RoleExistsAsync(UsersRoles.Admin))
                await roleManager.CreateAsync(new IdentityRole(UsersRoles.Admin));
            if (!await roleManager.RoleExistsAsync(UsersRoles.User))
                await roleManager.CreateAsync(new IdentityRole(UsersRoles.User));

            if (await roleManager.RoleExistsAsync(UsersRoles.Admin))
            {
                await userManager.AddToRoleAsync(user, UsersRoles.Admin);
            }
            return true;
        }

        public async Task<JwtSecurityToken> Login(LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.Username);
            if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:ValidIssuer"],
                    audience: _configuration["JWT:ValidAudience"],
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return token;
            }
            throw new StatusException { Status = "Error", Message = "User Login failed! Please check user is registered." };
        }

        public async Task<bool> Register(RegisterModel model)
        {
            var userExists = await userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                throw new StatusException{Status = "Error", Message = "User already exists!" };

            IdentityUser user = new IdentityUser()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                throw new StatusException{ Status = "Error", Message = "User creation failed! Please check user details and try again." };
            return true;
        }
    }
}
