using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VIPRentals.Models;

namespace VIPRentals.Data
{
    public class VIPRentalsContext : DbContext
    {
        public VIPRentalsContext (DbContextOptions<VIPRentalsContext> options)
            : base(options)
        {
        }

        public DbSet<VIPRentals.Models.Vehicle> Vehicle { get; set; } = default!;
        public DbSet<VIPRentals.Models.Rental> Rental { get; set; } = default!;
        public DbSet<VIPRentals.Models.Review> Review { get; set; } = default!;
    }
}
