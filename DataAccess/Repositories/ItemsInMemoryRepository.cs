using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

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

		public List<IItemValidating> Get()
		{
			return _cache.Get<List<IItemValidating>>(KEY) ?? new List<IItemValidating>();
		}

		public void Save(List<IItemValidating> items)
		{
			_cache.Set(KEY, items);
		}

		// For memory repo, Add == Save
		public void Add(List<IItemValidating> items)
		{
			Save(items);
		}
		public void Approve(List<string> ids)
		{
			
		}
	}
}

