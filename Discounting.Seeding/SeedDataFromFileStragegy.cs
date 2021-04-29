using System.Linq;
using Discounting.Extensions;

namespace Discounting.Seeding
{
    public class SeedDataFromFileStrategy<T> : ISeedDataStrategy<T> where T : class
    {
        public T[] GetSeedData()
        {
            var filename = $"{typeof(T).Name}.json";
            var content = filename.ReadEmbeddedResource<Seeder>();
            var list = Newtonsoft.Json.JsonConvert.DeserializeObject<T[]>(content);

            return list.ToArray();
        }
    }
}
