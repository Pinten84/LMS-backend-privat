using LMS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Data.Configurations;
public class ApplicationUserConfigurations : IEntityTypeConfiguration<ApplicationUser>
{
            public void Configure(EntityTypeBuilder<ApplicationUser> builder)
            {
                builder.ToTable("ApplicationUser");
                // Add more configurations here
            }
}
