using Domain.Interfaces;
using Domain.Models;
using DataAccess.Context;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Repositories
{
    public class ItemsDbRepository : ItemsRepository
    {
        private readonly AppDbContext _db;

        public ItemsDbRepository(AppDbContext db)
        {
            _db = db;
        }

        // Generic Get (not really used for step 6, but for interface)
        public List<IItemValidating> Get()
        {
            var restaurants = _db.Restaurants
                .Where(r => r.Status == "Approved")
                .Cast<IItemValidating>();

            return restaurants.Concat(menuItems).ToList();
        }

        // For compatibility with interface - delegate to Add
        public void Save(List<IItemValidating> items)
        {
            Add(items);
        }

        
        public void Add(List<IItemValidating> items)
        {
            foreach (var item in items)
            {
                if (item is Restaurant r)
                {
                    if (string.IsNullOrEmpty(r.Status))
                        r.Status = "Approved";   

                    _db.Restaurants.Add(r);
                }
                else if (item is MenuItem m)
                {
                    if (string.IsNullOrEmpty(m.Status))
                        m.Status = "Approved";

                    _db.MenuItems.Add(m);
                }
            }

            _db.SaveChanges();
        }

        public void Approve(List<string> ids)
        {
            foreach (var id in ids)
            {
                if (int.TryParse(id, out var restId))
                {
                    var r = _db.Restaurants.Find(restId);
                    if (r != null)
                        r.Status = "Approved";
                }
                else if (Guid.TryParse(id, out var menuId))
                {
                    var m = _db.MenuItems.Find(menuId);
                    if (m != null)
                        m.Status = "Approved";
                }
            }

            _db.SaveChanges();
        }
    }
}
