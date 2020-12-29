# Covariance and contravariance by example.
Covariance and contravariance are very spooky words.

I use these words if want to sound very smart - and it usually works. No one really knows what the hell it is. 

As I want to explain this topic as simple as possible (but not simpler) I'll try to avoid these smart sounding words.

I'm going to implement a Repository pattern with use of generics to explain this topic. 

I'll use C# for my examples, it's pretty similar in languages like Java or Kotlin. 

## You already know half of it!

Let's define few simple classes. 

```csharp
record Person(string Name);
record Employee(string Name) : Person(Name);
record RemoteEmployee(string Name, string location) : Employee(Name);
```
Create few employees and put them inside a collection.

```csharp
IEnumerable<Employee> people = new List<Employee>
{
    new Employee("Andrew"),
    new RemoteEmployee("Karen","USA")
};
```
Since an `Employee` is also a `Person` I should be able to do this.
```csharp
IEnumerable<Person> people = new List<Employee> // It's not <Employee> anymore.
{
    new Employee("Andrew"),
    new RemoteEmployee("Karen","USA")
};
```

So if a `Employee` is also a `Person` I can use a more generic type on the left side of the assignment. AKA Covariance!

Simple right?

![told you](assets/toldya.gif)

Since I mentioned a *generic type* let's make something where we can make a use of generics. Let's save the data in a database.

Should I use MySQL, Excel, SQL Server, SQLite or Postgres? The answer is - we shouldn't have to care. All I want to do is insert some data and get it back later. Here a repository pattern comes to rescue!  

```csharp
interface IRepository<T>     // "T" stands for "data of some type"
{
    void Insert(T item);     // I want to insert some data.
    T Get(string id);        // and get it 
    IEnumerable<T> GetAll(); //            back later
}
```

I implemented a very simple repository which stores the object in a Json file on a disk. You can see it [here](FileRepository.cs).

When you implement a repository like this you encounter a problem while implementing the `Insert` method. 
How to insert the data in such a way that I can retrieve it later? 
There's an `id` in the `Get` method but under what `id` would you Insert the data?

You have multiple options here 
    1. Let a database handle it - The database could generate some Guid (like Mongo) or a relational database could generate primary key.
    2. Use a hash code or a to string of an object - But you may never get the object back, so never do that.
    3. Enforce an Id before it is added to a database.
    4. Some other idea.

You're in charge, not a database or a hash function so I'm going with the option #3.

![you are the king](assets/king.gif)

I want all my persistent objects to implement an `Entity` interface. 

```csharp
interface Entity { string Id { get; } }

record Person(string Name) : Entity { public string Id => Name; } // Name is not the best Id of course.
```

I also have to enforce the rule on the interface itself.

```csharp
interface IRepository<T> where T : Entity  // "T" stands for "data of some type" where the data also has an Id.
```

Now I can store my list of employees in a database! 

```csharp
static void Main(string[] args)
{
    var employeesRepository = new FileRepository<Employee>();
    AddEmployees(employeesRepository);  // Add a list of employees to a repository
    ReadAndPrintRepository(employeesRepository);     // Read the repository and print users.
}

static void AddEmployees(IRepository<Employee> repository) 
    => new List<Employee>
        {
            new RemoteEmployee("Karen","Usa"),
            new Employee("Karen")
        }.ForEach(repository.Insert);

static void ReadAndPrintRepository(IRepository<Employee> repository) 
    => repository
        .GetAll()
        .ToList()
        .ForEach(Console.WriteLine);
```

This all works, which is great. But an Employee is also a Person so I'd like to change make a small change.

```csharp
static void ReadAndPrintRepository(IRepository<Person> repository)
```

Suddenly you get an error

```
cannot convert from 'FileRepository<Employee>' to 'IRepository<Person>'csharp(CS1503)
```


![but why meme](assets/but-why.gif)


But why? It works on the example above!

To quote myself  "So if a `Employee` is also a `Person` I can use a more abstract type on the left side of the assignment. AKA Covariance!

What's the difference? Well if you take a look on the IEnumerable (or Collection in Java) interface it's defined like [this.](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.ienumerable-1?view=net-5.0)

```csharp
public interface IEnumerable<out T> : System.Collections.IEnumerable
```

The `out` keyword is crucial here. It says to the compiler - use the type `T`or a more abstract/derived type. The type `T` is a *covariant* parameter. It allows some change, a variance.
Similar in Java you'd write `Collection<? extends T>` 

But why the `out` keyword? Why not `in` or `covariant` or `leave me alone and work plz`?

Let's add an `out` keyword to our interface and let's see.

```
Invalid variance: The type parameter 'T' must be contravariantly valid on 'IRepository<T>.Insert(T)'. 'T' is covariant. csharp(CS1961)
```
(╯°□°）╯︵ ┻━┻

Whaaat?  

Well. When you specify the `out` keyword it means that the interface can only **OUTPUT** the the type `T`. It cannot take an input of type `T`, like it does in the Insert method. 

I'll remove the `Insert` method from the `IRepository` interface and move it to `IWriteOnlyRepository`. To have some symmetry in the codebase I'll introduce an `IReadOnlyRepository` and merge them in the `IRepository`.

```csharp
interface IWriteOnlyRepository<T>
{
    void Insert(T item);
}

interface IReadOnlyRepository<out T>
{
    T Get(string id);
    IEnumerable<T> GetAll();
}

interface IRepository<T> : IWriteOnlyRepository<T>, IReadOnlyRepository<T> where T : Entity
{
}
```

Now when you use the `IReadOnlyRepository` interface as a parameter in the `ReadAndPrintRepository` method you won't get an error anymore! 

┬─┬ノ( º _ ºノ)
```csharp
static void Main(string[] args)
{
    var employeesRepository = new FileRepository<Employee>();
    AddEmployees(employeesRepository);
    ReadAndPrintRepository(employeesRepository);
}

static void ReadAndPrintRepository(IReadOnlyRepository<Person> repository) => repository.GetAll().ToList().ForEach(Console.WriteLine);

static void AddEmployees(IRepository<Employee> repository) => new List<Employee>
    {
        new Employee("Bjork"),
        new RemoteEmployee("Karen","Usa")
    }.ForEach(repository.Insert);
```

Everything works as expected. Life's good. But what if I add a special method which will only handle RemoteEmployees?

```csharp
static void AddRemoteEmployees(IRepository<RemoteEmployee> repository)
    {
        //some operations only valid for RemoteEmployee
        //OnlyRemoteEmployeeMethod(remoteEmployee)
        repository.Insert(new RemoteEmployee("Andrew","Canada"));
        repository.Insert(new RemoteEmployee("Carol","UK"));
    }
```

Unfortunately you'll get this error 
`cannot convert from 'FileRepository<Employee>' to 'IRepository<RemoteEmployee>'`. 

The `FileRepository` implements the interface `IRepository<Employee>` so only an Employee can be added into the repository. You can say that is very rigid. It wont allow any variance, therefore it is **Invariant**.

The `IWriteOnlyRepository` has currently the same problem as `IRepository` it does not allow any variance. Since we have an `out` modifier on the `IReadOnlyRepository`, don't we have an `in` modifier we can use? 

Yes, we do! When you specify the `in` keyword it means that the interface can only accept an **INPUT** of the type `T`.  Since we are only using the type `T` as an input and we never return in we can mark the input type as contravariant using the `in` keyword.

Similar in Java you'd write `List<? super T>` 

```csharp
interface IWriteOnlyRepository<in T>
{
    void Insert(T item);
}
```

Last thing to do is to change the interface in the method. Since `IRepository` doesn't allow any variance, we have to use a more flexible interface which is the `IWriteOnlyRepository`.  `RemoteEmployee` is more specific than `Employee`, but since our `IWriteOnlyRepository` is marked with an `out` keyword it doesn't mind more specific types. AKA contravariance.

```csharp
static void AddRemoteEmployees(IWriteOnlyRepository<RemoteEmployee> repository) => new List<RemoteEmployee>
    {
        new RemoteEmployee("Andrew","Canada"),
        new RemoteEmployee("Carol","UK")
    }.ForEach(repository.Insert);
```

It works! 
(☞ﾟヮﾟ)☞   

## Summary

When your interface allows you to use a single type, it's offers no flexibility, no variance, therefore it's *invariant*.

If you go through a list of employees and you want to treat them as `Person`, thanks to covariance you can.

If tou want to add a manager to a list of employees, you can treat the list of employees as if it was a list of managers, because you're only appending to the list and employee list should be able to handle a manager.


I hope I made this weird topic a bit more clear! 




