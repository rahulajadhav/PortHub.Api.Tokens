using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortHub.Api.Tokens.Data.Models;
using PortHub.Api.Tokens.Data.Models.viewModels;

namespace PortHub.Api.Tokens.Data
{
    public class appDbContext : IdentityDbContext<ApplicationUserVM>
    {
        public appDbContext(DbContextOptions<appDbContext> options) : base(options)
        {
            
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ApplicationUserVM> app { get; set; }
    }
}
