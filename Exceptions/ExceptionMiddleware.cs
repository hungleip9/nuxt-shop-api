using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using nuxt_shop.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace nuxt_shop.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                await HandleExceptionAsync(httpContext, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = exception switch
            {
                LQUnAuthorizeException => 401,
                LQException => 410,
                LQForbiddenException => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            };
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers.Add("Cache-Control", "no-cache");
            context.Response.StatusCode = code;
            var data = JsonConvert.SerializeObject(new
            {
                messages = exception.Message,
                success = false,
                code
            }, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            if (exception is not LQUnAuthorizeException && exception is not LQException)
            {
                Console.WriteLine(exception);
            }
            await context.Response.WriteAsync(data);
        }
    }
}
