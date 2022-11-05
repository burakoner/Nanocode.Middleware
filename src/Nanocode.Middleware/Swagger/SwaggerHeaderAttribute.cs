using System;

namespace Nanocode.Middleware.Swagger
{
    public class SwaggerHeaderAttribute : Attribute
    {
        public string HeaderName { get; }
        public string Description { get; }
        public string DefaultValue { get; }
        public bool IsRequired { get; }

        public SwaggerHeaderAttribute(string headerName, string description = null, string defaultValue = null, bool isRequired = false)
        {
            this.HeaderName = headerName;
            this.Description = description;
            this.DefaultValue = defaultValue;
            this.IsRequired = isRequired;
        }
    }
}
