using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces;
using DataAccess.Factories;
using System.IO;

namespace WebUI.Controllers
{
    public class BulkImportController : Controller
    {
        private readonly ItemsRepository _memoryRepo;
        private readonly ImportItemFactory _factory;

        public BulkImportController(
            [FromKeyedServices("memory")] ItemsRepository memoryRepo,
            ImportItemFactory factory)
        {
            _memoryRepo = memoryRepo;
            _factory = factory;
        }

        // STEP 1: Show upload form
        public IActionResult BulkImport()
        {
            return View();
        }

        // STEP 2: Accept uploaded JSON file
        [HttpPost]
        public IActionResult BulkImport(IFormFile jsonFile)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a JSON file.");
                return View();
            }

            string json;

            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = reader.ReadToEnd();
            }

            var items = _factory.Create(json);
            _memoryRepo.Save(items);

            TempData["msg"] = "Items imported into memory successfully!";
            return RedirectToAction("BulkImport");
        }
    }
}
