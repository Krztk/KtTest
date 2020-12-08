using KtTest.Infrastructure.Data;
using KtTest.Models;
using KtTest.Results;
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
            var result = new OperationResult<string>();
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var signInResult = await signInManager.PasswordSignInAsync(user, password, false, false);
            if (signInResult.Succeeded)
            {
                result.Data = GenerateToken(user);
                return result;
            }

            result.AddFailure(Failure.BadRequest("Username and password don't match."));
            return result;
        }

        public async Task<OperationResult> RegisterOrganizationOwner(string email, string username, string password)
        {
            var user = new AppUser { UserName = username, Email = email, IsTeacher = true };
            return await CreateUser(password, user);
        }

        public async Task<OperationResult> RegisterUser(string code, string email, string username, string password)
        {
            var result = new OperationResult();
            var invitation = await dbContext.Invitations.FirstOrDefaultAsync(x => x.Code == code);
            if (invitation == null)
            {
                result.AddFailure(Failure.BadRequest());
                return result;
            }

            var user = new AppUser
            {
                UserName = username,
                Email = email,
                InvitedBy = invitation.InvitedBy,
                IsTeacher = invitation.IsTeacher
            };

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

            var result = new OperationResult();
            if (createUserResult.Succeeded)
            {
                return result;
            }
            else
            {
                foreach (var error in createUserResult.Errors)
                {
                    result.AddFailure(Failure.BadRequest($"{error.Code}: {error.Description}"));
                }

                return result;
            }
        }

        private string GenerateToken(AppUser user)
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
                issuer: "http://localhost:5000",
                audience: "http://localhost:5000",
                claims: claims,
                expires: DateTime.Now.AddMinutes(25),
                signingCredentials: signinCredentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }
    }
}

