using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MRT.Application.Interfaces;
using MRT.Application.Repositories;
using MRT.Application.Utils;
using MRT.Contract.DTOs.Auth;
using MRT.Contract.DTOs.User;
using MRT.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly IAuthRepository _authRepository;
        private readonly TokenGenerators _tokenGenerators;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IRedisService _redisService;
        private readonly IUserRepository _userRepository;

        public AuthService(IMapper mapper, IAuthRepository authRepository, TokenGenerators tokenGenerators, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IRedisService redisService, IUserRepository userRepository)
        {
            _mapper = mapper;
            _authRepository = authRepository;
            _tokenGenerators = tokenGenerators;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _redisService = redisService;
            _userRepository = userRepository;
        }

        public async Task<Authenticator> LoginAsync(LoginDTO loginDTO)
        {
            try
            {
                var user = await _userRepository.GetUserByEmail(loginDTO.Email);

                if (user == null)
                {
                    throw new KeyNotFoundException("Invalid email or account does not exist.");
                }

                if (!user.IsVerified)
                {
                    throw new InvalidOperationException("Account is not activated. Please verify your email.");
                }
                if (user.Status.ToString() != "Active")
                {
                    throw new InvalidOperationException("Your account has been lock. Contact to website to solve it.");
                }
                if (!BCrypt.Net.BCrypt.Verify(loginDTO.PasswordHash, user.PasswordHash))
                {
                    throw new UnauthorizedAccessException("Invalid password.");
                }

                // Generate JWT token
                var token = await GenerateJwtToken(user);
                return token;
            }
            catch (KeyNotFoundException ex)
            {
                // Handle cases where the user is not found
                throw new ApplicationException("Invalid email or account does not exist.", ex);
            }
            catch (InvalidOperationException ex)
            {
                // Handle cases where the account is not verified
                throw new ApplicationException("Account is not activated. Please verify your email.", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Handle cases where the password is invalid
                throw new ApplicationException("Invalid password.", ex);
            }
            catch (Exception ex)
            {
                // General exception handling
                throw new ApplicationException("An error occurred during login.", ex);
            }
        }

        private async Task<Authenticator> GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString()), // Ensuring UserId claim is added
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.RoleName),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(120), // Token expiration set to 120 minutes
            signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid().ToString();
            await _authRepository.UpdateRefreshToken(user.Id, refreshToken);

            return new Authenticator
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken
            };
        }

        public async Task<Authenticator> RefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new ArgumentNullException(nameof(token), "Refresh token is missing");

            var checkRefreshToken = _tokenGenerators.ValidateRefreshToken(token);
            if (!checkRefreshToken)
                return null;

            var user = await _authRepository.GetRefreshToken(token);
            if (user == null)
                return null;

            if (user.Role == null)
                throw new Exception("User role is missing");

            List<Claim> claims = new()
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Role.RoleName ?? "")
            };

            var (accessToken, refreshToken) = _tokenGenerators.GenerateTokens(claims);

            await _authRepository.UpdateRefreshToken(user.Id, refreshToken);

            return new Authenticator
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        public async Task<bool> DeleteRefreshToken(Guid userId)
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var expiry = jwtToken.ValidTo;

            await _redisService.AddToBlacklistAsync(token, expiry);

            return await _authRepository.DeleteRefreshToken(userId);
        }

        public async Task<string> GetIdFromToken()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
                throw new Exception("Token not found");

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                throw new Exception("Invalid token");

            var userId = jwtToken.Claims.First(claim => claim.Type == "id").Value;

            return userId;
        }
    }
}
