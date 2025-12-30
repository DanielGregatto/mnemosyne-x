namespace Domain.Core.Events
{
    public class EntityDeletedEvent<T> : IEntityDeletedEvent<T> where T : EntityBase<T>
    {
        public T Entity { get; set; }

        public EntityDeletedEvent(T entity)
        {
            this.Entity = entity;
        }
    }
}
