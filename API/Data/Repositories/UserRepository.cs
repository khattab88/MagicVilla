using API.Data.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Models;
using Models.Dtos.Identity;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        private string secretKey;

        public UserRepository(ApplicationDbContext db, IConfiguration configuration,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;

            secretKey = configuration.GetValue<string>("ApiSettings:SecretKey");
        }

        public bool IsUnique(string userName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == userName);

            if (user == null)
            {
                return true;
            }

            return false;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto loginRequest)
        {
            var user = _db.ApplicationUsers
                .FirstOrDefault(u => u.UserName.ToLower() == loginRequest.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);

            if (user == null || isValid == false)
            {
                return new LoginResponseDto()
                {
                    User = null,
                    Token = null
                };
            }

            var roles = await _userManager.GetRolesAsync(user);

            LoginResponseDto response = new ()
            {
                User = _mapper.Map<UserDto>(user),
                Role = roles.FirstOrDefault(),
            };

            // if user is found, generate jwt token
            var jwtTokenId = $"JTI{Guid.NewGuid().ToString()}";
            response.Token.AccessToken =  await GenerateAccessToken(user, jwtTokenId);
            response.Token.RefreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);

            return response;
        }

        public async Task<TokenDto> RefreshAccessToken(TokenDto token)
        {
            // find existing refresh token
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.RefreshToken);
            if(existingRefreshToken == null) 
            { 
                return new TokenDto(); 
            }

            // comapare date from both refresh, access tokens provided, if any mismatch happened consider it a FRAUD
            var accessTokenData = GetAccessTokenData(token.AccessToken);
            if (!accessTokenData.isSuccess || 
                accessTokenData.userId != existingRefreshToken.UserId || 
                accessTokenData.tokenId != existingRefreshToken.JwtTokenId)
            {
                existingRefreshToken.IsValid = false;
                _db.SaveChanges();

                return new TokenDto();
            }

            // when someone tries an invalid refresh token, consider it a possible fraud
            // => invalidate all token chain
            if (!existingRefreshToken.IsValid)
            {
                var tokenChain = _db.RefreshTokens.Where(t => t.UserId == existingRefreshToken.UserId
                && t.JwtTokenId == existingRefreshToken.JwtTokenId);

                foreach (var t in tokenChain)
                {
                    t.IsValid = false;
                }
                _db.UpdateRange(tokenChain);
                await _db.SaveChangesAsync();

                return new TokenDto();
            }

            // if refresh token is expired, just mark it as invalid and return empty => force login again
            if(existingRefreshToken.ExpiresAt <  DateTime.Now) 
            {
                existingRefreshToken.IsValid = false;
                _db.SaveChanges();

                return new TokenDto();
            }

            // if token is valid => replace it with new refresh token and with updated expiry date
            var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

            // revoke old refresh token => mark it invalid
            existingRefreshToken.IsValid = false;
            _db.SaveChanges();

            // generate new access, refresh tokens
            var applicationUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == existingRefreshToken.UserId);
            if(applicationUser == null)
            {
                return new TokenDto();
            }

            var newAccessToken = await GenerateAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

            return new TokenDto() 
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task<UserDto> Register(RegisterRequestDto registerRequest)
        {
            ApplicationUser user = new()
            {
                Name = registerRequest.Name,
                UserName = registerRequest.UserName,
                Email = registerRequest.UserName,
                NormalizedEmail = registerRequest.UserName.ToUpper(),
                // Password = registerRequest.Password,
                // Role = registerRequest.Role,
            };

            // await _db.LocalUsers.AddAsync(user);
            // await _db.SaveChangesAsync();
            //user.Password = "";

            try
            {
                var result = await _userManager.CreateAsync(user, registerRequest.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, registerRequest.Role);

                    var userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == registerRequest.UserName);

                    return _mapper.Map<UserDto>(userFromDb);
                }
            }
            catch (Exception ex)
            {

                throw;
            }

            return new UserDto();
        }

        public async Task RevokeRefreshToken(TokenDto token)
        {
            var existingRefreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token.RefreshToken);

            // Compare data from existing refresh and access token provided and
            // if there is any missmatch then we should do nothing with refresh token

            var isTokenValid = GetAccessTokenData(token.AccessToken, existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
            if (!isTokenValid)
            {
                return;
            }

            await MarkAllTokenInChainAsInvalid(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);
        }


        private async Task<string> GenerateAccessToken(ApplicationUser user, string jwtTokenId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, (await _userManager.GetRolesAsync(user)).FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti, jwtTokenId),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                }),
                Expires = DateTime.Now.AddMinutes(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature),
                Issuer = "https://magicvilla-api.com",
                Audience = "https://test-magic-api.com"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
        {
            RefreshToken refreshToken = new()
            {
                IsValid = true,
                UserId = userId,
                JwtTokenId = tokenId,
                ExpiresAt = DateTime.Now.AddMinutes(5),
                Token = Guid.NewGuid() + "-" + Guid.NewGuid()
            };

            await _db.RefreshTokens.AddAsync(refreshToken);
            await _db.SaveChangesAsync();

            return refreshToken.Token;
        }

        private (bool isSuccess, string userId, string tokenId) GetAccessTokenData(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);

                var userId = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
                var tokenId = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

                return (true, userId, tokenId);
            }
            catch (Exception)
            {
                return (false, null, null);
            }
        }

        private bool GetAccessTokenData(string accessToken, string expectedUserId, string expectedTokenId)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(accessToken);
                var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
                var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
                return userId == expectedUserId && jwtTokenId == expectedTokenId;

            }
            catch
            {
                return false;
            }
        }


        private async Task MarkAllTokenInChainAsInvalid(string userId, string tokenId)
        {
            await _db.RefreshTokens.Where(u => u.UserId == userId
               && u.JwtTokenId == tokenId)
                   .ExecuteUpdateAsync(u => u.SetProperty(refreshToken => refreshToken.IsValid, false));

        }


        private Task MarkTokenAsInvalid(RefreshToken refreshToken)
        {
            refreshToken.IsValid = false;
            return _db.SaveChangesAsync();
        }
    }
}
