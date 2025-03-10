using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class SwaggerAuthHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Security == null)
            operation.Security = new List<OpenApiSecurityRequirement>();

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };

        operation.Security.Add(new OpenApiSecurityRequirement
        {
            { scheme, new List<string>() }
        });

        // ✅ Kiểm tra nếu Authorization header đã tồn tại
        var authParameter = operation.Parameters?.FirstOrDefault(p => p.Name == "Authorization");
        if (authParameter != null)
        {
            authParameter.Description = "Nhập token mà KHÔNG cần 'Bearer '. Hệ thống sẽ tự động thêm.";
            authParameter.Schema = new OpenApiSchema { Type = "string", Default = new Microsoft.OpenApi.Any.OpenApiString("Bearer ") };
        }
    }
}
