using System;
using System.Collections.Generic;

namespace Nanocode.Middleware.Swagger
{
    public class SwaggerAuthHeaderAttribute : Attribute
    {
        public List<SwaggerAuthHeaderParam> AuthHeaders { get; }

        public SwaggerAuthHeaderAttribute()
        {
            this.AuthHeaders = new List<SwaggerAuthHeaderParam>();
            this.AuthHeaders.Add(new SwaggerAuthHeaderParam("X-CE-APIKEY", "Header Item Description", "Default Value", true));
            this.AuthHeaders.Add(new SwaggerAuthHeaderParam("X-CE-SIGNATURE", "Header Item Description", "Default Value", true));
        }
    }

    public class SwaggerAuthHeaderParam
    {
        public string HeaderName { get; }
        public string Description { get; }
        public string DefaultValue { get; }
        public bool IsRequired { get; }

        public SwaggerAuthHeaderParam(string headerName, string description = null, string defaultValue = null, bool isRequired = false)
        {
            this.HeaderName = headerName;
            this.Description = description;
            this.DefaultValue = defaultValue;
            this.IsRequired = isRequired;
        }
    }
}
