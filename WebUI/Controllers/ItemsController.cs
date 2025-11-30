using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class ItemsController : Controller
    {
        // normal catalog (browsing)
        public IActionResult Catalog(bool approvalMode = false, string view = "cards")
        {
            ViewBag.ApprovalMode = approvalMode;
            ViewBag.ViewMode = view;

            // TODO: later load real items from repository
            var items = new List<ItemValidating>();

            return View(items); // uses Views/Items/Catalog.cshtml
        }

        // 🔥 this will be used when we come to SE3.3, but we stub it now
        [HttpPost]
        public IActionResult Approve(List<int> restaurantIds, List<Guid> menuItemIds)
        {
            // TODO: call ItemsDbRepository.Approve(...) later
            // For now, just redirect back to normal catalog
            return RedirectToAction("Catalog");
        }

        // Example: open the SAME Catalog view in "approval mode"
        public IActionResult Pending()
        {
            ViewBag.ApprovalMode = true;
            ViewBag.ViewMode = "cards";

            // TODO: later: load pending restaurants/menuItems from DB as IItemValidating
            var pendingItems = new List<ItemValidating>();

            return View("Catalog", pendingItems);
        }
    }
}
