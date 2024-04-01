using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Reflection.Metadata;
using TestApp8._0.Domain.Entities;

namespace TestApp8._0.Domain
{
        public class DataContext : DbContext
    {

            public DbSet<Student> Students { get; set; }


            protected readonly IConfiguration Configuration;

            public DataContext(IConfiguration configuration)
            {
                Configuration = configuration;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
            {
                // connect to sql server with connection string from app settings
                //options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"));
                if (!options.IsConfigured)
                {
                    options.UseSqlServer(Configuration.GetConnectionString("WebApiDatabase"))
                        .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()));
                    // or UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()));
                }
            }
            public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
            {
                // get added or updated entries
                var addedOrUpdatedEntries = ChangeTracker.Entries()
                        .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

                // fill out the audit fields
                foreach (var entry in addedOrUpdatedEntries)
                {
                    var entity = entry.Entity as AuditableEntity;

                    if (entry.State == EntityState.Added)
                    {

                        entity.CreatedBy = "system";
                        entity.CreatedOn = DateTime.UtcNow;

                    }
                    entity.UpdatedBy = "system";
                    entity.UpdatedOn = DateTime.UtcNow;
                }

                return base.SaveChangesAsync(cancellationToken);
            }
        

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
               
            }
        }
    }


