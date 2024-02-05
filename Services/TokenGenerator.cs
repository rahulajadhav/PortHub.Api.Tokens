using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PortHub.Api.Tokens.Data;
using PortHub.Api.Tokens.Data.Models;
using PortHub.Api.Tokens.Data.Models.ViewModel;
using PortHub.Api.Tokens.Data.Models.viewModels;
using PortHub.Api.Tokens.Interface;

namespace PortHub.Api.Tokens.Services
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly appDbContext _appDbContext;
        private readonly UserManager<ApplicationUserVM> _userManager;
        public TokenGenerator(IConfiguration configuration,
                TokenValidationParameters tokenValidationParameters,
                appDbContext appDbContext,
                UserManager<ApplicationUserVM> userManager)
        {
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
            _appDbContext = appDbContext;
            _userManager = userManager;

        }

        public async Task<(bool IsSuccess, AuthVM refreshToken, string ErrorMessage)> RefreshTokenGeneratorAsync(ApplicationUserVM userExists)
        {
            var result = await GenerateJWTTokenAsync(userExists, null);
            return (true, result, "SUCCESS");
        }



        private async Task<AuthVM> GenerateJWTTokenAsync(ApplicationUserVM userExists, RefreshToken rtoken)
        {

            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userExists.UserName),
                new Claim(ClaimTypes.NameIdentifier, userExists.Id),
                new Claim(JwtRegisteredClaimNames.Email, userExists.Email),
                new Claim(JwtRegisteredClaimNames.Sub, userExists.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var AuthSigngingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issure"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddMinutes(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(AuthSigngingKey, SecurityAlgorithms.HmacSha256)
                );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            if (rtoken != null)
            {
                var rTokenResponse = new AuthVM()
                {
                    Token = jwtToken,
                    ExpiresAt = token.ValidTo,
                    Emailid = userExists.Id,
                    RefreshToken = rtoken.Token,
                };

                return rTokenResponse;

            }

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsRevoked = false,
                UserId = userExists.Id,
                DateAdded = DateTime.UtcNow,
                DateExpire = DateTime.UtcNow.AddMonths(6),
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
            };
            await _appDbContext.RefreshTokens.AddAsync(refreshToken);
            await _appDbContext.SaveChangesAsync();
            var response = new AuthVM()
            {
                Token = jwtToken,
                ExpiresAt = token.ValidTo,
                Emailid = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value,
                RefreshToken = refreshToken.Token,
            };

            return response;

        }


        public async Task<(bool isSuccess, string ErrorMessage, AuthVM auth)> RefreshTokenVM(RefreshTokenVM refereshTokenVM)
        {
            var result = VerifyAndGenerateTokenAsync(refereshTokenVM);
            if (result.Result.isSuccess)
            {
                return (true, "Success", result.Result.authVM);
            }
            else {
                return (false, "Failed", null);
            }
            
        }

        private async Task<(AuthVM authVM, bool isSuccess)> VerifyAndGenerateTokenAsync(RefreshTokenVM verifyToken)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var storedToken = await _appDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == verifyToken.RefreshToken);
            var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
            try
            {
                var tokenCheckResult = jwtTokenHandler.ValidateToken(verifyToken.Token, _tokenValidationParameters, out var validatedToken);
                
                var response = await GenerateJWTTokenAsync(dbUser, storedToken);
                return (response, true); 
            }
            catch (SecurityTokenExpiredException)
            {
                if (storedToken.DateExpire >= DateTime.UtcNow)
                {
                    var response =  await GenerateJWTTokenAsync(dbUser, storedToken);
                    return (response,true);
                }
                else
                {
                    var response =  await GenerateJWTTokenAsync(dbUser, null);
                    return (response, true);
                }
            }
        }

      
    }
}
