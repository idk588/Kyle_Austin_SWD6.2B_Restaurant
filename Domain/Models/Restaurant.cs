using Domain.Interfaces;
using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class Restaurant : IItemValidating
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string OwnerEmailAddress { get; set; }

        public string Status { get; set; } = "Pending";
        public string? ImageUrl { get; set; }


        public List<string> GetValidators()
        {

            return new List<string>();
        }
        public string GetCardPartial()
        {
            return "RestaurantCard";
        }

    }
}
