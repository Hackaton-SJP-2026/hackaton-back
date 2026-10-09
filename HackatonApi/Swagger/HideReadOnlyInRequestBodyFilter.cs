using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HackatonApi.Swagger
{
    public sealed class HideReadOnlyInRequestBodyFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.RequestBody?.Content is null) return;

            foreach (var mediaType in operation.RequestBody.Content.Values)
            {
                if (mediaType.Schema?.Reference is null) continue;

                var refId = mediaType.Schema.Reference.Id;
                if (!context.SchemaRepository.Schemas.TryGetValue(refId, out var original))
                    continue;

                var readOnlyKeys = original.Properties
                    .Where(p => p.Value.ReadOnly)
                    .Select(p => p.Key)
                    .ToHashSet(StringComparer.Ordinal);

                if (readOnlyKeys.Count == 0) continue;

                var requestSchemaName = refId + "Input";
                if (!context.SchemaRepository.Schemas.ContainsKey(requestSchemaName))
                {
                    context.SchemaRepository.Schemas[requestSchemaName] = new OpenApiSchema
                    {
                        Type = original.Type,
                        Description = original.Description,
                        AdditionalPropertiesAllowed = original.AdditionalPropertiesAllowed,
                        Properties = original.Properties
                            .Where(p => !readOnlyKeys.Contains(p.Key))
                            .ToDictionary(p => p.Key, p => p.Value),
                        Required = new HashSet<string>(
                            original.Required.Where(r => !readOnlyKeys.Contains(r)))
                    };
                }

                mediaType.Schema = new OpenApiSchema
                {
                    Reference = new OpenApiReference
                    {
                        Id = requestSchemaName,
                        Type = ReferenceType.Schema
                    }
                };
            }
        }
    }
}
