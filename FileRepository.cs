using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace variance
{
    class FileRepository<T> : IRepository<T> where T : Entity
    {
        public T Get(string id)
        {
            var filePath = Path.Join(Dir, $"{id}.json");
            var fileContent = File.ReadAllText(filePath);
            var parsedObject = Parse(fileContent);
            return parsedObject;
        }
        public IEnumerable<T> GetAll() => Directory
            .GetFiles(Dir)
            .Select(File.ReadAllText)
            .Select(Parse);
        public void Insert(T item)
        {
            var filePath = Path.Join(Dir, $"{item.Id}.json");
            var encodedObject = JsonConvert.SerializeObject(item);
            File.WriteAllText(filePath, encodedObject);
        }
        private T Parse(string serialized) => JsonConvert.DeserializeObject<T>(serialized);
        public string Dir { get; private set; }
        public FileRepository()
        {
            Dir = Path.Join("data", this.GetType().Name, typeof(T).Name);
            Directory.CreateDirectory(Dir);
        }

    }
}
