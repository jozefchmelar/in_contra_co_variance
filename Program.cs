using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace variance
{
    class Program
    {
        static void Main(string[] args)
        {
            var employeesRepository = new FileRepository<Employee>();
            AddEmployees(employeesRepository);
            AddRemoteEmployees(employeesRepository);
            ReadAndPrintRepository(employeesRepository);
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
                
        static void AddRemoteEmployees(IWriteOnlyRepository<RemoteEmployee> repository)
        {
            //some operations only valid for RemoteEmployee
            //OnlyRemoteEmployeeMethod(remoteEmployee)
            repository.Insert( new RemoteEmployee("Andrew","Canada"));
            repository.Insert( new RemoteEmployee("Carol","UK"));
        }
      

}
}