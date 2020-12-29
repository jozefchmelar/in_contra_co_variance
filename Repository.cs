using System;
using System.Collections.Generic;

namespace variance
{

    interface IWriteOnlyRepository<in T>
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

}
