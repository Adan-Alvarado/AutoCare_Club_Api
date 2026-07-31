using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoCare_Club.Api.Constants;
using AutoCare_Club.Api.Database;
using AutoCare_Club.Api.Entities;
using AutoCare_Club_Api.Dtos.Auth;
using AutoCare_Club_Api.Dtos.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AutoCare_Club_Api.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AutoCareDbContext _context;

        public AuthService(
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IConfiguration configuration,
            AutoCareDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task<ResponseDto<LoginResponseDto>> LoginAsync(
            LoginDto dto)
        {
            var userEntity = await _userManager.FindByEmailAsync(
                dto.Email.Trim());

            if (userEntity is null)
            {
                return UnauthorizedResponse(
                    "El correo o la contraseña son incorrectos.");
            }

            if (!userEntity.IsActive)
            {
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = HttpStatusCode.FORBIDDEN,
                    Status = false,
                    Message = "El usuario se encuentra inactivo."
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(
                userEntity,
                dto.Password,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return UnauthorizedResponse(
                    "El correo o la contraseña son incorrectos.");
            }

            var authClaims = await GetClaimsAsync(userEntity);
            var jwtToken = GetToken(authClaims);
            var refreshToken = GenerateRefreshTokenString();

            userEntity.RefreshToken = refreshToken;
            userEntity.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(
                GetRefreshTokenExpiryMinutes());

            await _context.SaveChangesAsync();

            return LoginResponse(
                userEntity,
                jwtToken,
                refreshToken,
                "Autenticación satisfactoria.");
        }

        public async Task<ResponseDto<LoginResponseDto>>
            RefreshTokenAsync(RefreshTokenDto dto)
        {
            ClaimsPrincipal principal;

            try
            {
                principal = GetPrincipalFromExpiredToken(dto.Token);
            }
            catch (SecurityTokenException)
            {
                return UnauthorizedResponse(
                    "El token de acceso no es válido.");
            }
            catch (ArgumentException)
            {
                return UnauthorizedResponse(
                    "El token de acceso no es válido.");
            }

            var userId =
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("UserId")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return UnauthorizedResponse(
                    "El token no contiene un usuario válido.");
            }

            var userEntity = await _userManager.FindByIdAsync(userId);

            if (userEntity is null)
            {
                return UnauthorizedResponse(
                    "No se encontró el usuario del token.");
            }

            if (!userEntity.IsActive)
            {
                return new ResponseDto<LoginResponseDto>
                {
                    StatusCode = HttpStatusCode.FORBIDDEN,
                    Status = false,
                    Message = "El usuario se encuentra inactivo."
                };
            }

            if (string.IsNullOrWhiteSpace(userEntity.RefreshToken) ||
                userEntity.RefreshToken != dto.RefreshToken ||
                userEntity.RefreshTokenExpiry is null ||
                userEntity.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                return UnauthorizedResponse(
                    "El refresh token no es válido o ha expirado.");
            }

            var authClaims = await GetClaimsAsync(userEntity);
            var jwtToken = GetToken(authClaims);
            var newRefreshToken = GenerateRefreshTokenString();

            userEntity.RefreshToken = newRefreshToken;
            userEntity.RefreshTokenExpiry = DateTime.UtcNow.AddMinutes(
                GetRefreshTokenExpiryMinutes());

            await _context.SaveChangesAsync();

            return LoginResponse(
                userEntity,
                jwtToken,
                newRefreshToken,
                "Token renovado correctamente.");
        }

        private async Task<List<Claim>> GetClaimsAsync(
            UserEntity userEntity)
        {
            var authClaims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    userEntity.Id),

                new Claim(
                    ClaimTypes.Email,
                    userEntity.Email ?? string.Empty),

                new Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),

                new Claim(
                    "UserId",
                    userEntity.Id)
            };

            var userRoles = await _userManager.GetRolesAsync(
                userEntity);

            foreach (var role in userRoles)
            {
                authClaims.Add(
                    new Claim(ClaimTypes.Role, role));
            }

            return authClaims;
        }

        private static string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using var numberGenerator =
                RandomNumberGenerator.Create();

            numberGenerator.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        private JwtSecurityToken GetToken(
            List<Claim> authClaims)
        {
            var secret = GetRequiredJwtSetting("JWT:Secret");

            var authSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret));

            var token = new JwtSecurityToken(
                issuer: GetRequiredJwtSetting("JWT:ValidIssuer"),
                audience: GetRequiredJwtSetting(
                    "JWT:ValidAudience"),
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(
                    GetAccessTokenExpiryMinutes()),
                claims: authClaims,
                signingCredentials: new SigningCredentials(
                    authSigningKey,
                    SecurityAlgorithms.HmacSha256));

            return token;
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(
            string token)
        {
            var secret = GetRequiredJwtSetting("JWT:Secret");

            var tokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,

                    // Se coloca false porque el access token
                    // puede estar vencido al renovarlo.
                    ValidateLifetime = false,

                    ValidIssuer = GetRequiredJwtSetting(
                        "JWT:ValidIssuer"),

                    ValidAudience = GetRequiredJwtSetting(
                        "JWT:ValidAudience"),

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secret)),

                    ClockSkew = TimeSpan.Zero
                };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new SecurityTokenException(
                    "El algoritmo del token no es válido.");
            }

            return principal;
        }

        private string GetRequiredJwtSetting(string key)
        {
            var value = _configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"Falta configurar {key}.");
            }

            return value;
        }

        private int GetAccessTokenExpiryMinutes()
        {
            var minutes =
                _configuration.GetValue<int>("JWT:Expires");

            return minutes > 0 ? minutes : 60;
        }

        private int GetRefreshTokenExpiryMinutes()
        {
            var minutes = _configuration.GetValue<int>(
                "JWT:RefreshTokenExpiry");

            // Siete días si no se configuró otro valor.
            return minutes > 0 ? minutes : 10080;
        }

        private static ResponseDto<LoginResponseDto> LoginResponse(
            UserEntity userEntity,
            JwtSecurityToken jwtToken,
            string refreshToken,
            string message)
        {
            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = HttpStatusCode.OK,
                Status = true,
                Message = message,
                Data = new LoginResponseDto
                {
                    Email = userEntity.Email ?? string.Empty,
                    Token = new JwtSecurityTokenHandler()
                        .WriteToken(jwtToken),
                    RefreshToken = refreshToken
                }
            };
        }

        private static ResponseDto<LoginResponseDto>
            UnauthorizedResponse(string message)
        {
            return new ResponseDto<LoginResponseDto>
            {
                StatusCode = HttpStatusCode.UNAUTHORIZED,
                Status = false,
                Message = message
            };
        }
    }
}