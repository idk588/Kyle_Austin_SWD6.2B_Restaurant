using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Domain.Models;
using Domain.Interfaces;

namespace DataAccess.Factories
{
    public class ImportItemFactory
    {
        public List<ItemValidating> Create(string json)
        {
            var items = new List<ItemValidating>();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var element in root.EnumerateArray())
            {
                string type = element.GetProperty("type").GetString();

                if (type.Equals("restaurant", StringComparison.OrdinalIgnoreCase))
                {
                    var restaurant = new Restaurant
                    {
                        Name = element.GetProperty("name").GetString(),
                        OwnerEmailAddress = element.GetProperty("ownerEmailAddress").GetString(),
                        Description = element.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        Address = element.TryGetProperty("address", out var addr) ? addr.GetString() : null,
                        Phone = element.TryGetProperty("phone", out var phone) ? phone.GetString() : null,
                        Status = "Pending"
                    };

                    items.Add(restaurant);
                }
                else if (type.Equals("menuItem", StringComparison.OrdinalIgnoreCase))
                {
                    var menuItem = new MenuItem
                    {
                        Title = element.GetProperty("title").GetString(),
                        Price = element.GetProperty("price").GetDecimal(),
                        RestaurantId = int.Parse(
                            element.GetProperty("restaurantId").GetString().Replace("R-", "")
                        ),
                        Status = "Pending"
                    };

                    items.Add(menuItem);
                }
            }

            return items;
        }
    }
}
