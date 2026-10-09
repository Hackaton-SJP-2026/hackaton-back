using System.ComponentModel;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HackatonApi.Swagger
{
    public sealed class ApplyReadOnlyAttributeSchemaFilter : ISchemaFilter
    {
        private const BindingFlags PropertyLookup =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties is null || schema.Properties.Count == 0) return;
            if (context.Type is null) return;

            foreach (var property in schema.Properties)
            {
                var propInfo = context.Type.GetProperty(property.Key, PropertyLookup);
                if (propInfo is null) continue;

                var readOnly = propInfo.GetCustomAttribute<ReadOnlyAttribute>();
                if (readOnly?.IsReadOnly == true)
                    property.Value.ReadOnly = true;
            }
        }
    }
}
