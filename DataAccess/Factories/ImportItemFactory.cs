using System;
using System.Collections.Generic;
using System.Text.Json;
using Domain.Interfaces;

namespace DataAccess.Factories
{
	public class ImportItemFactory
	{
		public List<IItemValidating> Create(string json)
		{
			var result = new List<IItemValidating>();

			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			if (root.ValueKind == JsonValueKind.Object)
			{
				AddRestaurantsFromObject(root, result);
				AddMenuItemsFromObject(root, result);
			}
			else if (root.ValueKind == JsonValueKind.Array)
			{
				foreach (var element in root.EnumerateArray())
				{
					if (element.TryGetProperty("restaurantId", out _))
					{
						var menuItem = CreateMenuItemFromElement(element);
						result.Add(menuItem);
					}
					else
					{
						var restaurant = CreateRestaurantFromElement(element);
						result.Add(restaurant);
					}
				}
			}

			return result;
		}

		// --------------- FROM OBJECT ROOT -------------------

		private void AddRestaurantsFromObject(JsonElement root, List<IItemValidating> result)
		{
			if (root.TryGetProperty("restaurants", out var restaurantsElement) &&
				restaurantsElement.ValueKind == JsonValueKind.Array)
			{
				foreach (var r in restaurantsElement.EnumerateArray())
				{
					var restaurant = CreateRestaurantFromElement(r);
					result.Add(restaurant);
				}
			}
		}

		private void AddMenuItemsFromObject(JsonElement root, List<IItemValidating> result)
		{
			if (root.TryGetProperty("menuItems", out var menuItemsElement) &&
				menuItemsElement.ValueKind == JsonValueKind.Array)
			{
				foreach (var m in menuItemsElement.EnumerateArray())
				{
					var menuItem = CreateMenuItemFromElement(m);
					result.Add(menuItem);
				}
			}
		}

		// --------------- HELPERS TO BUILD MODELS -------------------

		private Restaurant CreateRestaurantFromElement(JsonElement r)
		{
			return new Restaurant
			{
				Id = TryGetInt(r, "id"),
				Name = TryGetString(r, "name"),
				Description = TryGetString(r, "description"),
				Address = TryGetString(r, "address"),
				Phone = TryGetString(r, "phone"),
				OwnerEmailAddress = TryGetString(r, "ownerEmailAddress"),
				Status = "Pending",
				ImageUrl = null
			};
		}

		private MenuItem CreateMenuItemFromElement(JsonElement m)
		{
			return new MenuItem
			{
				Id = TryGetGuid(m, "id"),
				Title = TryGetString(m, "title"),
				Price = TryGetDecimal(m, "price"),
				RestaurantId = TryGetInt(m, "restaurantId"),
				Status = "Pending",
				ImageUrl = null
			};
		}

		// --------------- SAFE JSON READS (no exceptions) -------------------

		private static string TryGetString(JsonElement element, string propertyName)
		{
			return element.TryGetProperty(propertyName, out var prop)
				? prop.GetString() ?? string.Empty
				: string.Empty;
		}

		private static int TryGetInt(JsonElement element, string propertyName)
		{
			if (element.TryGetProperty(propertyName, out var prop))
			{
				if (prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var value))
					return value;

				if (prop.ValueKind == JsonValueKind.String &&
					int.TryParse(prop.GetString(), out var parsed))
					return parsed;
			}

			return 0;
		}

		private static Guid TryGetGuid(JsonElement element, string propertyName)
		{
			if (element.TryGetProperty(propertyName, out var prop) &&
				prop.ValueKind == JsonValueKind.String &&
				Guid.TryParse(prop.GetString(), out var g))
			{
				return g;
			}

			return Guid.NewGuid();
		}

		private static decimal TryGetDecimal(JsonElement element, string propertyName)
		{
			if (element.TryGetProperty(propertyName, out var prop))
			{
				if (prop.ValueKind == JsonValueKind.Number && prop.TryGetDecimal(out var d))
					return d;

				if (prop.ValueKind == JsonValueKind.String &&
					decimal.TryParse(prop.GetString(), out var parsed))
					return parsed;
			}

			return 0m;
		}
	}
}