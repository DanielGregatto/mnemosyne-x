using Domain.Core.Events.Interfaces;

namespace Domain.Core.Events
{
    /// <summary>
    /// Contract for entity update events.
    /// Triggered when an existing entity is modified in the system.
    /// </summary>
    /// <typeparam name="T">The entity type that was updated</typeparam>
    public interface IEntityUpdatedEvent<T> : IDomainEvent where T : EntityBase<T>
    {
        /// <summary>
        /// The entity that was updated
        /// </summary>
        T Entity { get; set; }
    }
}
