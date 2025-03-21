using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using nuxt_shop.Extensions;
using nuxt_shop.Exceptions;
using nuxt_shop.Repositories;
using System.Security;

namespace nuxt_shop.Filters
{
    public class UserFilterAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            if (user.Identity is { IsAuthenticated: true })
            {
                var _tokenLoginRepository = context.HttpContext.RequestServices.GetService<ITokenLoginRepository>() ?? throw new Exception("Có lỗi xảy ra");
                var _config = context.HttpContext.RequestServices.GetService<IConfiguration>() ?? throw new Exception("Có lỗi xảy ra");
                if (_tokenLoginRepository == null) throw new Exception("Có lỗi xảy ra");
                var userId = user.GetCustomerId();
                var currentLogin = await _tokenLoginRepository.GetAll().Where(i => i.UserId == userId).FirstOrDefaultAsync();
                if (currentLogin == null)
                {
                    throw new LQUnAuthorizeException("Bạn cần đăng nhập để truy cập tính năng này");
                }
                var authString = context.HttpContext.Request.Headers["Authorization"].ToString();
                var token = authString.Split(' ').LastOrDefault();
                if (token != currentLogin.AccessToken)
                {
                    throw new LQUnAuthorizeException(
                        "Tài khoản đang được sử dụng ở thiết bị khác, vui lòng đăng nhập lại");
                }

            }
            if (user.Identity?.IsAuthenticated != true)
            {
                throw new LQUnAuthorizeException("Bạn cần đăng nhập để truy cập tính năng này");
            }
            switch (await CheckPermission(context))
            {
                case PermissionResult.Pass:
                    await next();
                    break;
                case PermissionResult.AccountExpired:
                    throw new LQForbiddenException("Tài khoản đã hết hạn!");
                case PermissionResult.NoPermission:
                    throw new LQForbiddenException("Tài khoản không có quyền truy cập");
                case PermissionResult.AccessLimit:
                    throw new LQForbiddenException("Bạn đã vượt quá số lần truy cập của chức năng này!");
                case PermissionResult.InternalServerError:
                default:
                    throw new Exception("Có lỗi xảy ra");
            }
            await next();
        }
        private async Task<PermissionResult> CheckPermission(ActionContext context)
        {
            var _customer = context.HttpContext.RequestServices.GetService<IUserRepository>() ?? throw new Exception("Có lỗi xảy ra");
            var customerId = context.HttpContext.User.GetCustomerId();
            var expDate = context.HttpContext.User.GetCustomerExpireDate();
            if (expDate.Date < DateTime.Now.Date) return PermissionResult.AccountExpired;
            return PermissionResult.Pass;
        }
        public enum PermissionResult
        {
            Pass,
            AccountExpired,
            NoPermission,
            AccessLimit,
            InternalServerError
        }
    }

}
