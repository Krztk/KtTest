using KtTest.Dtos.Auth;
using KtTest.Results;
using KtTest.Services;
using System.Threading.Tasks;

namespace KtTest.Application_Services
{
    public class AuthOrchestrator
    {
        private readonly AuthService authService;

        public AuthOrchestrator(AuthService authService)
        {
            this.authService = authService;
        }

        public async Task<OperationResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            return (await authService.AuthenticateAndGetToken(loginDto.Username, loginDto.Password))
                .Then<LoginResponseDto>(x => new LoginResponseDto { Token = x });
        }

        public async Task<OperationResult<Unit>> RegisterOrganizationOwner(RegisterDto registerDto)
        {
            return await authService.RegisterOrganizationOwner(registerDto.Email, registerDto.Username, registerDto.Password);
        }

        public async Task<OperationResult<Unit>> RegisterRegularUser(string code, RegisterDto registerDto)
        {
            return await authService.RegisterUser(code, registerDto.Email, registerDto.Username, registerDto.Password);
        }
    }
}
