using LMS.API.Extensions;
using LMS.API.Middleware;
using Mapster;
using LMS.Shared;
using LMS.API.Services;
using LMS.Infrastructure.Data;
using LMS.Application.Mapping;

namespace LMS.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureSql(builder.Configuration);
        builder.Services.ConfigureControllers();

        builder.Services.AddMapster();
        MapsterConfiguration.Configure();
        builder.Services.AddRepositories();
        builder.Services.AddServiceLayer();

        builder.Services.ConfigureAuthentication(builder.Configuration);
        builder.Services.ConfigureIdentity();

        builder.Services.AddHostedService<DataSeedHostingService>();
        builder.Services.ConfigureCors();
        builder.Services.ConfigureOpenApi();

        var app = builder.Build();


        // Configure the HTTP request pipeline.
        // Custom standardized ProblemDetails middleware
        app.UseStandardProblemDetails();
        // Legacy exception handler (can be removed after full migration if redundant)
        app.ConfigureExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
