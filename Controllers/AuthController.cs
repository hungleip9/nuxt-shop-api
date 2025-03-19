using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using nuxt_shop.Dtos;
using nuxt_shop.Exceptions;
using nuxt_shop.Extensions;
using nuxt_shop.Models;
using nuxt_shop.Repositories;
using nuxt_shop.Services;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using nuxt_shop.Filters;

namespace nuxt_shop.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserLogRepository _userLogRepository;
        private readonly ITokenLoginRepository _tokenLoginRepository;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config, IUserRepository userRepository, IUserLogRepository userLogRepository, ITokenLoginRepository tokenLoginRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _userLogRepository = userLogRepository;
            _tokenLoginRepository = tokenLoginRepository;
            _tokenService = tokenService;
            _config = config;
        }
        [HttpPost("register")]
        public async Task<Result> Register(RegisterDto model)
        {
            if (!Regex.IsMatch(model.PhoneNumber, @"^0\d{9,10}$"))
            {
                throw new LQException("Số điện thoại không hợp lệ!");
            }
            if (!Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) {
                throw new LQException("Email không hợp lệ!");
            }
            var hasUserName = await _userRepository.CheckUserName(model.UserName);
            if (hasUserName)
            {
                throw new LQException("UserName đã tồn tại!");
            }
            var hasEmail = await _userRepository.CheckEmail(model.Email);
            if (hasEmail)
            {
                throw new LQException("Email đã tồn tại!");
            }
            var hasPhone = await _userRepository.CheckPhoneNumber(model.PhoneNumber);
            if (hasPhone)
            {
                throw new LQException("Số điện thoại đã tồn tại!");
            }
            var user = new User()
            {
                UserName = model.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsLocked = false,
            };
            await _userRepository.Register(user);
            var accessToken = GenerateAccessToken(user, out var expireTime);
            var refreshToken = GenerateRefreshToken();
            var tokenLogin = new TokenLogin()
            {
                UserId = user.Id,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
            };
            await _tokenLoginRepository.Create(tokenLogin);
            return new Result()
            {
                data = new
                {
                    info = user,
                    token = accessToken,
                    refresh = refreshToken,
                    expires = expireTime
                }
            };
        }

        [HttpPost("login")]
        public async Task<Result> Login(LoginDto model)
        {
            var user = await _userRepository.GetAll().Where(e => e.PhoneNumber == model.PhoneNumber).FirstOrDefaultAsync();
            if (user == null || string.IsNullOrEmpty(model.Password) || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                throw new LQException("Thông tin đăng nhập không chính xác");
            }
            var accessToken = GenerateAccessToken(user, out var expireTime);
            var refreshToken = GenerateRefreshToken();
            var updatePassword = string.IsNullOrEmpty(user.Password);

            var tokenLogin = await _tokenLoginRepository.GetAll()
                .Where(i => i.UserId == user.Id).FirstOrDefaultAsync();
            if (tokenLogin == null)
            {
                tokenLogin = new TokenLogin()
                {
                    UserId = user.Id,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,

                };
                await _tokenLoginRepository.Create(tokenLogin);
            }
            else
            {
                tokenLogin.AccessToken = accessToken;
                tokenLogin.RefreshToken = refreshToken;
                await _tokenLoginRepository.Update(tokenLogin);
            }

            var userInfo = await GetUserInfo(user.Id);
            return new Result()
            {
                code = HttpStatusCode.OK,
                messages = "Xác thực thành công",
                success = true,
                data = new
                {
                    info = userInfo,
                    token = accessToken,
                    refresh = refreshToken,
                    expires = expireTime,
                    updatepassword = updatePassword
                }
            };
        }
        [HttpPost("forgot")]
        public async Task<Result> Forgot(ForgotDto model)
        {
            var user = await _userRepository.GetAll().Where(e => e.PhoneNumber == model.PhoneNumber).FirstOrDefaultAsync();
            if (user == null)
            {
                throw new LQException("Thông tin đăng nhập không chính xác");
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            await _userRepository.Update(user);
            return new Result()
            {
                code = HttpStatusCode.OK,
                messages = "Thay đổi mật khẩu thành công!",
                success = true
            };
        }
        [HttpGet("info")]
        [Authorize]
        [UserFilter()]
        public async Task<Result> Info()
        {
            var userInfo = await GetUserInfo(User.GetCustomerId());
            return new Result()
            {
                code = HttpStatusCode.OK,
                messages = "Lấy thông tin user thành công!",
                success = true,
                data = new
                {
                    info = userInfo
                }
            };
        }
        [HttpPost("change-password")]
        [Authorize]
        [UserFilter()]
        public async Task<Result> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userRepository.GetAll().Where(e => e.PhoneNumber == model.PhoneNumber).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.OldPassword, user.Password))
            {
                throw new LQException("Thông tin đăng nhập không chính xác");
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            return new Result()
            {
                code = HttpStatusCode.OK,
                messages = "Thay đổi mật khẩu thành công!",
                success = true
            };
        }
        [HttpGet("logout/{refreshToken}")]
        public async Task<Result> Logout(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new LQException("Chưa có refreshToken!");
            }
            var tokenLogin = await _tokenLoginRepository.GetAll().Where(e => e.RefreshToken == refreshToken).FirstOrDefaultAsync();
            
            if (tokenLogin == null)
            {
                throw new LQException("refreshToken không tồn tại!");
            }
            await _tokenLoginRepository.Delete(tokenLogin);
            await _userLogRepository.AddUserLog(User.GetCustomerId(), $"Đăng xuất {tokenLogin}", "",
                    DateTime.Now, HttpContext.Request.GetIpAddress());
            return new Result()
            {
                code = HttpStatusCode.OK,
                messages = "Đăng xuất thành công",
                success = true
            };
        }
        [HttpGet("logout1")]
        public async Task<Result> Logout1()
        {
            return new Result()
            {
                code = HttpStatusCode.OK,
                messages = "Đăng xuất thành công",
                success = true
            };
        }
        private async Task<UserInfo?> GetUserInfo(int Id)
        {
            var userInfo = new UserInfo();
            var user = await _userRepository.GetInfoById(Id);
            if (user != null)
            {
                userInfo.Id = user.Id;
                userInfo.UserName = user.UserName;
                userInfo.FullName = user.FullName;
                userInfo.Email = user.Email;
                userInfo.Address = user.Address;
                userInfo.IdNumber = user.IdNumber;
                userInfo.DateOfBirth = user.DateOfBirth;
                userInfo.IsLocked = user.IsLocked;
                userInfo.Photo = user.Photo;
                userInfo.Status = user.Status;
            }
            return userInfo;
        }
        private string GenerateAccessToken(User user, out DateTime expireTime)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetValue<string>("AppSettings:SecretKey")));
            var expTime = _config.GetValue<int>("AppSettings:ExpiredTime");
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName ?? ""),
            };
            expireTime = DateTime.Now.AddMinutes(expTime);
            var tokeOptions = new JwtSecurityToken(
                claims: claims,
                expires: expireTime,
                signingCredentials: signinCredentials
            );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            expireTime = DateTime.Now.AddMinutes(expTime);
            return tokenString;
        }
        private string GenerateRefreshToken()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 32)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
