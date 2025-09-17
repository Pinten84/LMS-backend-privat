using LMS.API.Background;
using LMS.API.Swagger;
using LMS.Application.Contracts.Services;
using LMS.Application.Contracts.Repositories;
using LMS.Application;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Repositories;
using LMS.Presentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

namespace LMS.API.Extensions;
public static class ServiceExtensions
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            //ToDo: Restrict access to your BlazorApp only!
            options.AddDefaultPolicy(policy =>
            {
                //..
                //..
                //..
            });
            //Can be used during development
            options.AddPolicy("AllowAll", p =>
                p.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
        });
    }
    public static void ConfigureOpenApi(this IServiceCollection services) =>
services.AddEndpointsApiExplorer()
.AddSwaggerGen(setup =>
{
    setup.EnableAnnotations();
    setup.SchemaGeneratorOptions = new Swashbuckle.AspNetCore.SwaggerGen.SchemaGeneratorOptions();
    setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ange JWT som: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
    });
    // Global pagination parameter docs (applies when present)
    setup.OperationFilter<PaginationOperationFilter>();
    // ProblemDetails mapping for common error codes
    setup.MapType<ProblemDetails>(() => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["type"] = new() { Type = "string" },
            ["title"] = new() { Type = "string" },
            ["status"] = new() { Type = "integer", Format = "int32" },
            ["detail"] = new() { Type = "string" },
            ["instance"] = new() { Type = "string" }
        }
    });
});
    public static void ConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers(opt =>
        {
            opt.ReturnHttpNotAcceptable = true;
            opt.Filters.Add(new ProducesAttribute("application/json"));
        })
            .AddNewtonsoftJson()
            .AddApplicationPart(typeof(AssemblyReference).Assembly);
    }
    public static void ConfigureSql(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ApplicationDbContext") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContext' not found.")));
    }
    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<CourseRepository>();
        services.AddScoped<ModuleRepository>();
        services.AddScoped<ActivityRepository>();
        services.AddScoped<DocumentRepository>();
    }
    public static void AddServiceLayer(this IServiceCollection services)
    {
        services.AddScoped<LMS.Application.Contracts.IServiceManager, ServiceManager>();
        services.AddScoped<CourseService>();
        services.AddScoped<ModuleService>();
        services.AddScoped<ActivityService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<LMS.Application.Contracts.IAuthService, AuthService>();
        services.AddScoped(provider => new Lazy<LMS.Application.Contracts.IAuthService>(() => provider.GetRequiredService<LMS.Application.Contracts.IAuthService>()));
        // Hosted services
        services.AddHostedService<RefreshTokenCleanupService>();
        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("CourseTeacherOrAdmin", policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Teacher")));
            options.AddPolicy("ModuleCourseTeacherOrAdmin", policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Teacher")));
            options.AddPolicy("ActivityCourseTeacherOrAdmin", policy => policy.RequireAssertion(ctx =>
                ctx.User.IsInRole("Admin") || ctx.User.IsInRole("Teacher")));
            options.AddPolicy("DocumentOwnerOrAdmin", policy => policy.RequireAssertion(ctx =>
            {
                if (ctx.User.IsInRole("Admin"))
                    return true;
                // Simplified: expects a claim with DocumentOwnerId when evaluating resource (could be extended with IAuthorizationHandler)
                var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var docOwner = ctx.User.FindFirst("doc-owner-id")?.Value; // placeholder claim injection elsewhere
                return docOwner != null && docOwner == userId;
            }));
        });
    }
}
