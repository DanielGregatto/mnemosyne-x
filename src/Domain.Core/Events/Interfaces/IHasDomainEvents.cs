using System.Collections.Generic;

namespace Domain.Core.Events.Interfaces
{
    /// <summary>
    /// Marker interface for entities that support domain events
    /// </summary>
    public interface IHasDomainEvents
    {
        public IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
        public void RaiseDomainEvent(IDomainEvent evt);
        public void ClearDomainEvents();
    }
}
