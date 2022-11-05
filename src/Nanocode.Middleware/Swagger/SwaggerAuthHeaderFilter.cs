using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nanocode.Middleware.Swagger
{
    public class SwaggerAuthHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            if (context.MethodInfo.GetCustomAttribute(typeof(SwaggerAuthHeaderAttribute)) is SwaggerAuthHeaderAttribute attribute)
            {
                foreach (var item in attribute.AuthHeaders)
                {
                    var existingParam = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Header && p.Name == item.HeaderName);
                    if (existingParam != null) // remove description from [FromHeader] argument attribute
                    {
                        operation.Parameters.Remove(existingParam);
                    }

                    operation.Parameters.Add(new OpenApiParameter
                    {
                        Name = item.HeaderName,
                        In = ParameterLocation.Header,
                        Description = item.Description,
                        Required = item.IsRequired,
                        Schema = string.IsNullOrEmpty(item.DefaultValue)
                            ? null
                            : new OpenApiSchema
                            {
                                Type = "String",
                                Default = new OpenApiString(item.DefaultValue)
                            }
                    });
                }
            }
        }
    }
}
