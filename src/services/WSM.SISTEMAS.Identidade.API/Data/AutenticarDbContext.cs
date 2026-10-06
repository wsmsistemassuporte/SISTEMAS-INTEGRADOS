using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WSM.SISTEMAS.Identidade.API.Models;

namespace WSM.SISTEMAS.Identidade.API.Data
{
    public class AutenticarDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        public AutenticarDbContext(DbContextOptions<AutenticarDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
