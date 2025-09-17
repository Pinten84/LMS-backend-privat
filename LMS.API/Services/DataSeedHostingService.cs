using Bogus;
using LMS.Infrastructure.Data;
using LMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

//Add in secret.json
//{
//   "password" :  "YourSecretPasswordHere"
//}
public class DataSeedHostingService : IHostedService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly ILogger<DataSeedHostingService> logger;
    private UserManager<ApplicationUser> userManager = null!;
    private RoleManager<IdentityRole> roleManager = null!;
    private const string TeacherRole = "Teacher";
    private const string StudentRole = "Student";
    private const string AdminRole = "Admin";

    public DataSeedHostingService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DataSeedHostingService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        if (!env.IsDevelopment())
            return;

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var anyUsers = await context.Users.AnyAsync(cancellationToken);

        userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        ArgumentNullException.ThrowIfNull(roleManager, nameof(roleManager));
        ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

        try
        {
            // Always ensure required roles (idempotent)
            await AddRolesAsync([AdminRole, TeacherRole, StudentRole]);

            // Always ensure admin user exists (idempotent)
            await AddOrUpdateAdminUserAsync();

            if (!anyUsers)
            {
                await AddDemoUsersAsync();
                await AddUsersAsync(20);
                // Seed kurser, moduler, aktiviteter, dokument (initial content only once)
                await SeedLmsDataAsync(context);
                logger.LogInformation("Initial seed complete (demo users, random users, LMS data)");
            }
            else
            {
                logger.LogInformation("Users already exist: skipped demo/random user and LMS sample data seeding; ensured roles/admin user only.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Data seed fail with error: {ex.Message}");
            throw;
        }
    }

    private async Task AddRolesAsync(string[] rolenames)
    {
        foreach (string rolename in rolenames)
        {
            if (await roleManager.RoleExistsAsync(rolename))
                continue;
            var role = new IdentityRole { Name = rolename };
            var res = await roleManager.CreateAsync(role);

            if (!res.Succeeded)
                throw new Exception(string.Join("\n", res.Errors));
        }
    }
    private async Task AddDemoUsersAsync()
    {
        var teacher = new ApplicationUser
        {
            UserName = "teacher@test.com",
            Email = "teacher@test.com"
        };

        var student = new ApplicationUser
        {
            UserName = "student@test.com",
            Email = "student@test.com"
        };

        await AddUserToDb([teacher, student]);

        var teacherRoleResult = await userManager.AddToRoleAsync(teacher, TeacherRole);
        if (!teacherRoleResult.Succeeded)
            throw new Exception(string.Join("\n", teacherRoleResult.Errors));

        var studentRoleResult = await userManager.AddToRoleAsync(student, StudentRole);
        if (!studentRoleResult.Succeeded)
            throw new Exception(string.Join("\n", studentRoleResult.Errors));
    }

    private async Task AddUsersAsync(int nrOfUsers)
    {
        var faker = new Faker<ApplicationUser>("sv").Rules((f, e) =>
        {
            e.Email = f.Person.Email;
            e.UserName = f.Person.Email;
        });

        await AddUserToDb(faker.Generate(nrOfUsers));
    }

    private async Task AddOrUpdateAdminUserAsync()
    {
        var adminSection = configuration.GetSection("AdminSeed");
        var email = adminSection["Email"]; // required
        var userName = adminSection["UserName"] ?? email;
        var password = adminSection["Password"]; // required (dev only – recommend secrets for real usage)

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("AdminSeed configuration missing Email or Password; skipping admin user creation.");
            return;
        }

        var existing = await userManager.FindByEmailAsync(email);
        if (existing == null)
        {
            var adminUser = new ApplicationUser
            {
                Email = email,
                UserName = userName
            };
            var createRes = await userManager.CreateAsync(adminUser, password);
            if (!createRes.Succeeded)
            {
                throw new Exception("Failed creating admin user: " + string.Join("\n", createRes.Errors));
            }
            existing = adminUser;
            logger.LogInformation("Admin user created: {Email}", email);
        }
        else
        {
            // Could update properties if desired; keep minimal for now
            logger.LogInformation("Admin user already exists: {Email}", email);
        }

        if (!await userManager.IsInRoleAsync(existing, AdminRole))
        {
            var roleRes = await userManager.AddToRoleAsync(existing, AdminRole);
            if (!roleRes.Succeeded)
            {
                throw new Exception("Failed assigning admin role: " + string.Join("\n", roleRes.Errors));
            }
            logger.LogInformation("Admin role assigned to user {Email}", email);
        }
    }

    private async Task AddUserToDb(IEnumerable<ApplicationUser> users)
    {
        var passWord = configuration["password"];
        ArgumentNullException.ThrowIfNull(passWord, nameof(passWord));

        foreach (var user in users)
        {
            var result = await userManager.CreateAsync(user, passWord);
            if (!result.Succeeded)
                throw new Exception(string.Join("\n", result.Errors));
        }
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SeedLmsDataAsync(ApplicationDbContext context)
    {
        // Skapa 3 kurser med 2 moduler vardera, varje modul har 2 aktiviteter och 1 dokument
        var teacher = await context.Users.FirstOrDefaultAsync(u => u.Email == "teacher@test.com");
        var students = await context.Users.Where(u => u.Email != "teacher@test.com").Take(10).ToListAsync();
        var courses = new List<Course>();
        for (int i = 1; i <= 3; i++)
        {
            var course = new Course
            {
                Name = $"Kurs {i}",
                Description = $"Beskrivning för kurs {i}",
                StartDate = DateTime.Now.AddDays(-i * 10),
                Modules = new List<Module>(),
                Documents = new List<Document>(),
                Teacher = teacher,
                Students = students.Skip((i - 1) * 3).Take(3).ToList()
            };
            for (int j = 1; j <= 2; j++)
            {
                var module = new Module
                {
                    Name = $"Modul {j} i Kurs {i}",
                    Description = $"Beskrivning för modul {j}",
                    StartDate = DateTime.Now.AddDays(-j * 5),
                    EndDate = DateTime.Now.AddDays(j * 5),
                    Activities = new List<Activity>(),
                    Documents = new List<Document>()
                };
                for (int k = 1; k <= 2; k++)
                {
                    var activity = new Activity
                    {
                        Name = $"Aktivitet {k} i Modul {j}",
                        Type = k % 2 == 0 ? "Inlämning" : "Föreläsning",
                        Description = $"Beskrivning för aktivitet {k}",
                        StartTime = DateTime.Now.AddHours(-k * 2),
                        EndTime = DateTime.Now.AddHours(k * 2),
                        Documents = new List<Document>()
                    };
                    // Dokument till aktivitet
                    var activityDoc = new Document
                    {
                        Name = $"Dokument för Aktivitet {k}",
                        Description = "Aktivitetsdokument",
                        Timestamp = DateTime.Now,
                        UploadedByUserId = teacher?.Id ?? "",
                        LinkedEntityType = "Activity",
                        LinkedEntityId = k
                    };
                    activity.Documents.Add(activityDoc);
                    module.Activities.Add(activity);
                }
                var moduleDoc = new Document
                {
                    Name = $"Dokument för Modul {j}",
                    Description = "Moduldokument",
                    Timestamp = DateTime.Now,
                    UploadedByUserId = teacher?.Id ?? "",
                    LinkedEntityType = "Module",
                    LinkedEntityId = j
                };
                module.Documents.Add(moduleDoc);
                course.Modules.Add(module);
            }
            var courseDoc = new Document
            {
                Name = $"Dokument för Kurs {i}",
                Description = "Kursdokument",
                Timestamp = DateTime.Now,
                UploadedByUserId = teacher?.Id ?? "",
                LinkedEntityType = "Course",
                LinkedEntityId = i
            };
            course.Documents.Add(courseDoc);
            courses.Add(course);
        }
        await context.Courses.AddRangeAsync(courses);
        await context.SaveChangesAsync();
    }

}
