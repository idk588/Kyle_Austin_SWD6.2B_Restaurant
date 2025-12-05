using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Models
{
    public class Restaurant : IItemValidating
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string OwnerEmailAddress { get; set; }

        public string Status { get; set; } = "Pending";

        // Optional fields
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }

        //Image
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
