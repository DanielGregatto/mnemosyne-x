using Data.Context;
using Domain.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Data.UnitOfWork
{
    /// <summary>
    /// Unit of Work implementation - coordinates database transactions and domain event publishing
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IMediator _mediator;

        public UnitOfWork(AppDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        /// <summary>
        /// Saves all changes and publishes domain events
        /// </summary>
        public async Task<int> CommitAsync(CancellationToken ct = default)
        {
            // Save changes to database (triggers AppDbContext.SaveChangesAsync override)
            // This will automatically raise domain events based on entity state
            var result = await _context.SaveChangesAsync(ct);

            if (result > 0)
            {
                // Get all domain events from tracked entities
                var domainEvents = _context.GetDomainEvents();

                // Publish each domain event via MediatR
                foreach (var domainEvent in domainEvents)
                {
                    await _mediator.Publish(domainEvent, ct);
                }

                // Clear domain events from entities
                _context.ClearDomainEvents();
            }

            return result;
        }

        /// <summary>
        /// Saves changes without publishing domain events
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _context.SaveChangesAsync(ct);
        }
    }
}
