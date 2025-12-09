using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces;
using DataAccess.Repositories;
using Domain.Models;
using System.Collections.Generic;

namespace WebUI.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemsRepository _memoryRepo;
        private readonly ItemsDbRepository _dbRepo;

        public ItemsController(
            [FromKeyedServices("memory")] ItemsRepository memoryRepo,
            ItemsDbRepository dbRepo)
        {
            _memoryRepo = memoryRepo;
            _dbRepo = dbRepo;
        }

        // view for pending (in-memory) items – what you already used before
        public IActionResult Pending()
        {
            var items = _memoryRepo.Get();
            ViewBag.Mode = "Pending";
            return View("Catalog", items); 
        }

        //  show only APPROVED restaurants in card view
        public IActionResult Catalog()
        {
            List<Restaurant> approvedRestaurants = _dbRepo.GetApprovedRestaurants();
            return View("ApprovedCatalog", approvedRestaurants);
        }

        // show its APPROVED menu items in list view
        public IActionResult RestaurantMenu(int id)
        {
            List<MenuItem> menuItems = _dbRepo.GetApprovedMenuItemsByRestaurant(id);
            ViewBag.RestaurantId = id;
            return View("RestaurantMenuItems", menuItems);
        }
    }
}
