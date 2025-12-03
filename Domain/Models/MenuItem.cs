using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MenuItem : ItemValidating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; }

        [Range(0, 999)]
        public decimal Price { get; set; }

        [Required]
        public int RestaurantId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ImageUrl { get; set; }

        // ItemValidating 
        public List<string> GetValidators()
        {
            return new List<string>(); // Will be implemented later
        }

        public string GetCardPartial()
        {
            return "MenuItemCard";
        }
    }
}
