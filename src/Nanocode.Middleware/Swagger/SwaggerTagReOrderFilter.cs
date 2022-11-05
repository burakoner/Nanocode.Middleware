using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Nanocode.Middleware.Swagger
{
    public class SwaggerTagReOrderFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = swaggerDoc.Tags
                .OrderBy(tag => tag.Name)
                .ToList();
        }
    }
}
