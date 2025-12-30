namespace Domain.Core.Events
{
    public class EntityInsertedEvent<T> : IEntityInsertedEvent<T> where T : EntityBase<T>
    {
        public T Entity { get; set; }

        public EntityInsertedEvent(T entity)
        {
            this.Entity = entity;
        }
    }
}
