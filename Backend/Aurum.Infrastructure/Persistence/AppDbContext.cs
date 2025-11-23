using Microsoft.EntityFrameworkCore;
using Aurum.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        // Define DbSets for your entities here
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Invite> Invites { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


    }
}
