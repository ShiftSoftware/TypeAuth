using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Data;
using TypeAuth.AspNetCore.Sample.Server.Models;

namespace TypeAuth.AspNetCore.Sample.Server.Data.Configurations
{
    public class UserConfigurations : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(x => x.Username).HasMaxLength(255).IsRequired();
            builder.HasIndex(x => x.Username).IsUnique();
        }
    }
}
