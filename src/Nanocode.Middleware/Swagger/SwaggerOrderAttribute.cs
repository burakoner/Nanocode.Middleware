using System;

namespace Nanocode.Middleware.Swagger
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SwaggerOrderAttribute : Attribute
    {
        public int Order { get; }

        public SwaggerOrderAttribute(int order)
        {
            this.Order = order;
        }
    }
}
