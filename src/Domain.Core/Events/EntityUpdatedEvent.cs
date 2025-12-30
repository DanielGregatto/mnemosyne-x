namespace Domain.Core.Events
{
    public class EntityUpdatedEvent<T> : IEntityUpdatedEvent<T> where T : EntityBase<T>
    {
        public T Entity { get; set; }

        public EntityUpdatedEvent(T entity)
        {
            this.Entity = entity;
        }
        }
    }
