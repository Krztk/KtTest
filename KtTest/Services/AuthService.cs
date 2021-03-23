using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
using KtTest.Results.Errors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace KtTest.Services
{
    public class AuthService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly AppDbContext dbContext;
        private readonly IConfiguration configuration;

        public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext dbContext, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;
            this.configuration = configuration;
        }

        public async Task<OperationResult<string>> AuthenticateAndGetToken(string username, string password)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new BadRequestError();
            }

            var signInResult = await signInManager.PasswordSignInAsync(user, password, false, false);
            if (signInResult.Succeeded)
            {
                return GenerateToken(user);
            }

            return new BadRequestError("Username and password don't match.");
        }

        public async Task<OperationResult> RegisterOrganizationOwner(string email, string username, string password)
        {
            var user = AppUser.CreateOrganizationOwner(username, email);
            return await CreateUser(password, user);
        }

        public async Task<OperationResult> RegisterUser(string code, string email, string username, string password)
        {
            var result = new OperationResult();
            var invitation = await dbContext.Invitations.FirstOrDefaultAsync(x => x.Code == code);
            if (invitation == null)
            {
                return new BadRequestError();
            }

            var user = AppUser.CreateOrganizationMember(email, username, invitation.IsTeacher, invitation.InvitedBy);
            result = await CreateUser(password, user);
            if (result.Succeeded)
            {
                dbContext.Invitations.Remove(invitation);
                await dbContext.SaveChangesAsync();
            }
            return result;
        }

        private async Task<OperationResult> CreateUser(string password, AppUser user)
        {
            var createUserResult = await userManager.CreateAsync(user, password);

            if (createUserResult.Succeeded)
            {
                return OperationResult.Ok();
            }
            else
            {
                var sb = new StringBuilder();
                var prefix = "";
                foreach (var error in createUserResult.Errors)
                {
                    sb.Append($"{prefix}{error.Code}: {error.Description}");
                    prefix = ", ";
                }

                return new BadRequestError(sb.ToString());
            }
        }

        public string GenerateToken(AppUser user)
        {

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Symmetric:Key"]));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.IsTeacher)
            {
                claims.Add(new Claim("Employee", "1"));
            }

            if (!user.InvitedBy.HasValue)
            {
                claims.Add(new Claim("Owner", "1"));
            }

            var tokeOptions = new JwtSecurityToken(
                issuer: configuration["Jwt:ValidIssuer"],
                audience: configuration["Jwt:ValidAudience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(25),
                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }
    }
}

