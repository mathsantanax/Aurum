using Aurum_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurum_Infrastructure.Persistence
{
    public class InfraContext : DbContext
    {
        public InfraContext(DbContextOptions<InfraContext> options) 
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Wallet> Wallet { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Income> Income { get; set; }
        public DbSet<Cost> Cost { get; set; }


    }
}
