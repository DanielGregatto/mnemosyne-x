using Domain.Core.Events.Interfaces;

namespace Domain.Core.Events
{
    /// <summary>
    /// Contract for entity insertion events.
    /// Triggered when a new entity is created in the system.
    /// </summary>
    /// <typeparam name="T">The entity type that was inserted</typeparam>
    public interface IEntityInsertedEvent<T> : IDomainEvent where T : EntityBase<T>
    {
        /// <summary>
        /// The entity that was inserted
        /// </summary>
        T Entity { get; set; }
    }
}
