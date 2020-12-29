namespace variance
{
    abstract record Person(string Name) : Entity { public string Id => Name; }
    record Employee(string Name) : Person(Name);
    record RemoteEmployee(string Name, string location) : Employee(Name);
}
