using Domain.Core.Events;
using Domain.Core.Events.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Data.Context
{
    public class AppDbContext : DbContext
    {
        private readonly string _schema;
        private readonly string _defaultSchema;
        private readonly IConfiguration _configuration;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            IConfiguration configuration) : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _schema = _configuration["Schema"];
            _defaultSchema = _configuration["DefaultSchema"];
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(_defaultSchema);
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conection = _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(conection))
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

                optionsBuilder.UseSqlServer(conection);
            }
        }

        /// <summary>
        /// Automatically raises domain events for inserted, updated, and deleted entities before saving
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Get all entities that support domain events and have state changes
            var entitiesWithEvents = ChangeTracker.Entries()
                .Where(e => e.Entity is IHasDomainEvents)
                .Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Modified ||
                           e.State == EntityState.Deleted)
                .ToList();

            // Raise appropriate domain events based on entity state
            foreach (var entry in entitiesWithEvents)
            {
                var entity = (IHasDomainEvents)entry.Entity;
                var entityType = entry.Entity.GetType();

                // Create generic event type based on entity type
                switch (entry.State)
                {
                    case EntityState.Added:
                        var insertedEventType = typeof(EntityInsertedEvent<>).MakeGenericType(entityType);
                        var insertedEvent = (IDomainEvent)Activator.CreateInstance(insertedEventType, entry.Entity);
                        entity.RaiseDomainEvent(insertedEvent);
                        break;

                    case EntityState.Modified:
                        var updatedEventType = typeof(EntityUpdatedEvent<>).MakeGenericType(entityType);
                        var updatedEvent = (IDomainEvent)Activator.CreateInstance(updatedEventType, entry.Entity);
                        entity.RaiseDomainEvent(updatedEvent);
                        break;

                    case EntityState.Deleted:
                        var deletedEventType = typeof(EntityDeletedEvent<>).MakeGenericType(entityType);
                        var deletedEvent = (IDomainEvent)Activator.CreateInstance(deletedEventType, entry.Entity);
                        entity.RaiseDomainEvent(deletedEvent);
                        break;
                }
            }

            // Save changes to database
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets all domain events from tracked entities
        /// </summary>
        public IEnumerable<IDomainEvent> GetDomainEvents()
        {
            return ChangeTracker.Entries()
                .Where(e => e.Entity is IHasDomainEvents)
                .Select(e => e.Entity as IHasDomainEvents)
                .Where(e => e.DomainEvents.Any())
                .SelectMany(e => e.DomainEvents)
                .ToList();
        }

        /// <summary>
        /// Clears domain events from all tracked entities
        /// </summary>
        public void ClearDomainEvents()
        {
            ChangeTracker.Entries()
                .Where(e => e.Entity is IHasDomainEvents)
                .Select(e => e.Entity as IHasDomainEvents)
                .Where(e => e.DomainEvents.Any())
                .ToList()
                .ForEach(e => e.ClearDomainEvents());
        }
    }
}