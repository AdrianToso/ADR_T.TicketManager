using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ADR_T.TicketManager.Core.Domain.Entities;
using ADR_T.TicketManager.Core.Domain.Interfaces;
using ADR_T.TicketManager.Infrastructure.Persistence.Identity;
using System;
using System.Threading; 
using System.Threading.Tasks; 

namespace ADR_T.TicketManager.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }

        void IDbContext.Add<TEntity>(TEntity entity) where TEntity : class
        {
            base.Add(entity);
        }

        void IDbContext.Update<TEntity>(TEntity entity) where TEntity : class
        {
            base.Update(entity);
        }

        void IDbContext.Remove<TEntity>(TEntity entity) where TEntity : class
        {
            base.Remove(entity);
        }

        async Task<TEntity?> IDbContext.FindAsync<TEntity>(params object[] keyValues) where TEntity : class
        {
            return await base.FindAsync<TEntity>(keyValues);
        }

        // Sobrecarga SaveChangesAsync de IDbContext
        async Task<int> IDbContext.SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}