using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Security.Models;

namespace Security.Data
{
    public class SecurityDbContext : DbContext
    {
        public SecurityDbContext (DbContextOptions<SecurityDbContext> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comment { get; set; }

        public DbSet<AddFile> AddFile { get; set; }
    }
}
