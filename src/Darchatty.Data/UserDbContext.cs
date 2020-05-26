using System;
using Darchatty.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Darchatty.Data
{
    public class UserDbContext : IdentityDbContext<UserDao>
    {
        public UserDbContext(DbContextOptions options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.UsePostgresNamingConventions();
        }
    }
}
