using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Interfaces;
using Domain.Models;
using DataAccess.Context;

namespace DataAccess.Repositories
{
    public class ItemsDbRepository : ItemsRepository
    {
        private readonly AppDbContext _db;

        public ItemsDbRepository(AppDbContext db)
        {
            _db = db;
        }

        public List<ItemValidating> Get()
        {
            var restaurants = _db.Restaurants.ToList<ItemValidating>();
            var menuItems = _db.MenuItems.ToList<ItemValidating>();

            return restaurants.Concat(menuItems).ToList();
        }

        public void Save(List<ItemValidating> items)
        {
            foreach (var item in items)
            {
                if (item is Restaurant r)
                    _db.Restaurants.Add(r);

                if (item is MenuItem m)
                    _db.MenuItems.Add(m);
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
                    if (r != null) r.Status = "Approved";
                }
                else if (Guid.TryParse(id, out var mId))
                {
                    var m = _db.MenuItems.Find(mId);
                    if (m != null) m.Status = "Approved";
                }
            }

            _db.SaveChanges();
        }
    }
}
