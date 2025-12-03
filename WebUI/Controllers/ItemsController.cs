using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ItemsRepository _repo;

        public ItemsController([FromKeyedServices("memory")] ItemsRepository repo)
        {
            _repo = repo;
        }

        // Normal catalog
        public IActionResult Catalog(bool approvalMode = false, string view = "cards")
        {
            ViewBag.ApprovalMode = approvalMode;
            ViewBag.ViewMode = view;

            var items = _repo.Get();   // ← Load imported items

            return View(items);
        }

        // Approve action (used later)
        [HttpPost]
        public IActionResult Approve(List<int> restaurantIds, List<Guid> menuItemIds)
        {
            // Later will call ItemsDbRepository
            return RedirectToAction("Catalog");
        }

        // Pending catalog (later used in approval)
        public IActionResult Pending()
        {
            ViewBag.ApprovalMode = true;
            ViewBag.ViewMode = "cards";

            var pendingItems = _repo.Get();  // ← Load imported items

            return View("Catalog", pendingItems);
        }
    }
}
