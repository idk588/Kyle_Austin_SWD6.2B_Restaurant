using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Domain.Interfaces;

namespace DataAccess.Repositories
{
    public class ItemsInMemoryRepository : ItemsRepository
    {
        private readonly IMemoryCache _cache;
        private const string KEY = "ImportedItems";

        public ItemsInMemoryRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public List<ItemValidating> Get()
        {
            return _cache.Get<List<ItemValidating>>(KEY) ?? new List<ItemValidating>();
        }

        public void Save(List<ItemValidating> items)
        {
            _cache.Set(KEY, items);
        }

        public void Approve(List<string> ids)
        {
            // not needed in memory
        }
    }
}
