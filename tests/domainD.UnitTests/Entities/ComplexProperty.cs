namespace domainD.UnitTests.Entities
{
    public class ComplexProperty : Entity<string>
    {
        public ComplexProperty(string id) : base(id)
        {
        }

        public string Name { get; internal set; }

        public void SetName(string name)
        {
            RaiseEvent(new NameSet(name));
        }
    }

    public class NameSet : DomainEvent
    {
        public NameSet(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
