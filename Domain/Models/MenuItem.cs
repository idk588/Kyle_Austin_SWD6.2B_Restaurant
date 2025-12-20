using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class MenuItem : IItemValidating
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; }

        public decimal Price { get; set; }

        [Required]
        public int RestaurantId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? ImageUrl { get; set; }


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
