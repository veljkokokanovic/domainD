using System;

namespace domainD
{
    public abstract class Entity<TId> : Entity, IEquatable<Entity<TId>>
        where TId : IEquatable<TId>
    {
        protected Entity(TId id)
        {
            if (Equals(id, default(TId)))
            {
                throw new ArgumentNullException(nameof(id));
            }

            Identity = id;
        }

        public TId Identity { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is Entity<TId> entity)
            {
                return Equals(entity);
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        public bool Equals(Entity<TId> other)
        {
            return other != null && Identity.Equals(other.Identity);
        }
    }
}
