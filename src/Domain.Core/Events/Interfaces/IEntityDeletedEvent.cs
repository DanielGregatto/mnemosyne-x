using Domain.Core.Events.Interfaces;

namespace Domain.Core.Events
{
    /// <summary>
    /// Contract for entity deletion events.
    /// Triggered when an entity is deleted (soft or hard delete) in the system.
    /// </summary>
    /// <typeparam name="T">The entity type that was deleted</typeparam>
    public interface IEntityDeletedEvent<T> : IDomainEvent where T : EntityBase<T>
    {
        /// <summary>
        /// The entity that was deleted
        /// </summary>
        T Entity { get; set; }
    }
}
