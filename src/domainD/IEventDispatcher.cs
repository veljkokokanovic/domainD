namespace domainD
{
    public interface IEventDispatcher
    {
        void DispatchEvent(DomainEvent @event);
    }
}
