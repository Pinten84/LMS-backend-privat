using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.API.Swagger;

/// <summary>
/// Adds documentation for standard pagination query parameters if a matching signature is detected.
/// </summary>
public class PaginationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null)
            return;

        // If action already exposes page/pageSize in its parameters, enrich descriptions
        foreach (var param in operation.Parameters)
        {
            if (param.Name.Equals("page", StringComparison.OrdinalIgnoreCase))
                param.Description ??= "1-baserad sidindex (standard 1)";
            if (param.Name.Equals("pageSize", StringComparison.OrdinalIgnoreCase))
                param.Description ??= "Antal objekt per sida (standard t.ex. 10/20)";
            if (param.Name.Equals("search", StringComparison.OrdinalIgnoreCase))
                param.Description ??= "Frivillig fritextsökning";
        }
    }
}
