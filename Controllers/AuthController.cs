using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using nuxt_shop.Dtos;
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

namespace nuxt_shop.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserLogRepository _userLogRepository;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config, IUserRepository userRepository, IUserLogRepository userLogRepository, TokenService tokenService)
        {
            _userRepository = userRepository;
            _userLogRepository = userLogRepository;
            _tokenService = tokenService;
            _config = config;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (ModelState.IsValid)
            {
                if (request.Password != request.PasswordConfirm)
                {
                    return BadRequest(new
                    {
                        code = System.Net.HttpStatusCode.BadRequest,
                        success = false,
                        message = "Mật khẩu không trùng nhau!"
                    });
                }
                if (!Regex.IsMatch(request.PhoneNumber, @"^0\d{9,10}$"))
                {
                    return BadRequest(new
                    {
                        code = System.Net.HttpStatusCode.BadRequest,
                        success = false,
                        message = "Số điện thoại không hợp lệ!"
                    });
                }
                if (!Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) {
                    return BadRequest(new
                    {
                        code = System.Net.HttpStatusCode.BadRequest,
                        success = false,
                        message = "Email không hợp lệ!"
                    });
                }
                //var hasUserName = await _userRepository.CheckExistence(request.UserName, UserName);
                var hasUserName = await _userRepository.CheckUserName(request.UserName);
                if (hasUserName)
                {
                    return BadRequest(new
                    {
                        code = System.Net.HttpStatusCode.BadRequest,
                        success = false,
                        message = "UserName đã tồn tại!"
                    });
                }
                var hasEmail = await _userRepository.CheckEmail(request.Email);
                if (hasEmail)
                {
                    return BadRequest(new
                    {
                        code = System.Net.HttpStatusCode.BadRequest,
                        success = false,
                        message = "Email đã tồn tại!"
                    });
                }
                var hasPhone = await _userRepository.CheckEmail(request.PhoneNumber);
                if (hasPhone)
                {
                    return BadRequest(new
                    {
                        code = System.Net.HttpStatusCode.BadRequest,
                        success = false,
                        message = "Số điện thoại đã tồn tại!"
                    });
                }
                var user = new User();
                user.UserName = request.UserName;
                user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                user.Email = request.Email;
                user.PhoneNumber = request.PhoneNumber;
                user.IsLocked = false;
                await _userRepository.Register(user);
                return Ok(new
                {
                    code = System.Net.HttpStatusCode.OK,
                    success = true
                });
            }
            else
            {
                return BadRequest(new
                {
                    code = System.Net.HttpStatusCode.BadRequest,
                    success = false,
                    message = "Dữ liệu không hợp lệ!",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _userRepository.Authenticate(request.PhoneNumber, request.Password);
            if (user == null) return Unauthorized("Invalid credentials");
            var accessToken = GenerateAccessToken(user, out var expireTime);
            var refreshToken = GenerateRefreshToken();
            var userInfo = await GetUserInfo(user.Id);
            return Ok( new{
                code = HttpStatusCode.OK,
                messages = "Xác thực thành công",
                success = true,
                data = new
                {
                    info = userInfo,
                    token = accessToken,
                    refresh = refreshToken,
                }
            });
        }
        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> Info()
        {
            var userInfo = await GetUserInfo(User.GetCustomerId());
            return Ok(new
            {
                code = HttpStatusCode.OK,
                messages = "Lấy thông tin user thành công!",
                success = true,
                data = userInfo
            });
        }
        [HttpGet("logout")]
        [HttpGet("logout/{refreshToken}")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            if (User.Identity?.IsAuthenticated != true) return Ok(new
            {
                messages = "Đăng xuất thành công",
                success = true
            });

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _userLogRepository.AddUserLog(User.GetCustomerId(), "Đăng xuất", "",
                    DateTime.Now, HttpContext.Request.GetIpAddress());
            }
            return Ok(new
            {
                messages = "Đăng xuất thành công",
                success = true
            });
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
