using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MRT.Application.Interfaces;
using MRT.Application.Repositories;
using MRT.Application.Utils;
using MRT.Contract.DTOs.User;
using MRT.Contracts.Abstractions.Shared;
using MRT.Domain.Entities;
using MRT.Domain.Enums;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;
        private readonly TokenGenerators _tokenGenerators;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRedisService _redisService;
        private readonly IClaimsService _claimsService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IConfiguration configuration, IAuthRepository authRepository, TokenGenerators tokenGenerators, IHttpContextAccessor httpContextAccessor, IRedisService redisService, IClaimsService claimsService, IMapper mapper)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _authRepository = authRepository;
            _tokenGenerators = tokenGenerators;
            _httpContextAccessor = httpContextAccessor;
            _redisService = redisService;
            _claimsService = claimsService;
            _mapper = mapper;
        }

        public async Task<Result<object>> CreateNewUser(CreateUserDTO user)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (token == null)
                    return new Result<object>() { Error = 1, Message = "Token not found", Data = null };

                var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return new Result<object>() { Error = 1, Message = "Invalid token", Data = null };
                var userId = Guid.Parse(jwtToken.Claims.First(claim => claim.Type == "id").Value);

                var existingUser = await _userRepository.FindByEmail(user.Email);
                if (existingUser != null)
                {
                    throw new Exception("User already exists with this email!");
                }
                user.PasswordHash = HashPassword(user.PasswordHash);
                var userEntity = _mapper.Map<ApplicationUser>(user);
                userEntity.Status = StatusEnum.Active;
                userEntity.CreationDate = DateTime.Now;
                userEntity.CreatedBy = userId;
                var createdUser = await _userRepository.AddAsync(userEntity);
                return new Result<object>()
                {
                    Error = 0,
                    Message = "User created successfully!",
                    Data = user
                };
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx &&
                                   (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                var message = sqlEx.Message;

                if (message.Contains("IX_ApplicationUser_Email"))
                    throw new Exception("Email đã tồn tại.");
                else if (message.Contains("IX_ApplicationUser_UserName"))
                    throw new Exception("Tên người dùng đã tồn tại.");
                else if (message.Contains("IX_ApplicationUser_PhoneNumber"))
                    throw new Exception("Số điện thoại đã tồn tại.");
                else
                    throw new Exception("Lỗi dữ liệu: trùng thông tin.");
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public async Task<IList<ApplicationUser>> GetALl()
        {
            var getUser = await _userRepository.GetAllAsync();
            return getUser;
        }

        public async Task<UserDTO> GetUserById(Guid id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                throw new Exception("User is not exist!");
            }

            UserDTO userDto = new()
            {
                UserName = user.UserName,
                Email = user.Email,
            };

            return userDto;
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            await _userRepository.UpdateAsync(user);
        }
        public async Task<ApplicationUser> GetByEmail(string email)
        {
            return await _userRepository.GetUserByEmail(email);
        }

        public async Task<Result<UserDTO>> GetCurrentUserById()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token == null)
                return new Result<UserDTO>() { Error = 1, Message = "Token not found", Data = null };

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return new Result<UserDTO>() { Error = 1, Message = "Invalid token", Data = null };
            var userId = Guid.Parse(jwtToken.Claims.First(claim => claim.Type == "id").Value);
            var result = _mapper.Map<UserDTO>(await _userRepository.GetAllUserById(userId));
            return new Result<UserDTO>() { Error = 0, Message = "Get Information Successfully", Data = result };
        }

    }
}
