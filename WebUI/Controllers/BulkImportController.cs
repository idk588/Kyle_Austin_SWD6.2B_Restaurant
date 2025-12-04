using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Domain.Interfaces;
using DataAccess.Factories;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace WebUI.Controllers
{
    public class BulkImportController : Controller
    {
        // use the interface, not a concrete class
        private readonly ItemsRepository _memoryRepo;
        private readonly ImportItemFactory _factory;

        public BulkImportController(
            [FromKeyedServices("memory")] ItemsRepository memoryRepo,
            ImportItemFactory factory)
        {
            _memoryRepo = memoryRepo;
            _factory = factory;
        }

        // STEP 1 → Show upload page
        [HttpGet]
        public IActionResult BulkImport()
        {
            return View();
        }

        // STEP 2 → Upload JSON + Create ZIP (AA4.3)
        [HttpPost]
        public IActionResult BulkImport(IFormFile jsonFile)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid JSON file.");
                return View();
            }

            // Read file contents
            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = reader.ReadToEnd();
            }

            // Convert JSON → objects using assignment factory
            var items = _factory.Create(json);

            // Store data in memory (not DB yet)
            _memoryRepo.Save(items);

            // Create a temporary working directory
            var tempFolder = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempFolder);

            // Image placeholders numbering
            int restaurantCounter = 1;
            int menuCounter = 1;

            // Path to default image
            var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "default.jpg");

            // Build folder structure for ZIP
            foreach (var item in items)
            {
                string folderName;

                if (item.GetType().Name.ToLower().Contains("restaurant"))
                {
                    folderName = $"R-{restaurantCounter++}";
                }
                else
                {
                    folderName = $"M-{menuCounter++}";
                }

                var folderPath = Path.Combine(tempFolder, folderName);
                Directory.CreateDirectory(folderPath);

                System.IO.File.Copy(defaultImagePath, Path.Combine(folderPath, "default.jpg"), true);
            }

            // Create ZIP file
            var zipName = $"ImportedItems_{DateTime.Now:yyyyMMddHHmmss}.zip";
            var zipPath = Path.Combine(Path.GetTempPath(), zipName);

            if (System.IO.File.Exists(zipPath))
                System.IO.File.Delete(zipPath);

            ZipFile.CreateFromDirectory(tempFolder, zipPath);

            // ✅ Return ZIP download to user (no File() ambiguity)
            var bytes = System.IO.File.ReadAllBytes(zipPath);
            return new FileContentResult(bytes, "application/zip")
            {
                FileDownloadName = zipName
            };
        }

        // STEP 3 → Upload ZIP with updated images
        [HttpGet]
        public IActionResult UploadImages()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadImages(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid ZIP file.");
                return View();
            }

            // Temporary extract folder
            var extractPath = Path.Combine(Path.GetTempPath(), $"images-{Guid.NewGuid()}");
            Directory.CreateDirectory(extractPath);

            // Save uploaded zip
            var zipPath = Path.Combine(extractPath, "uploaded.zip");
            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                zipFile.CopyTo(stream);
            }

            // Extract files
            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // Get items currently in memory
            var items = _memoryRepo.Get();

            int restaurantCounter = 1;
            int menuCounter = 1;

            foreach (var item in items)
            {
                string folderName;

                if (item.GetType().Name.ToLower().Contains("restaurant"))
                {
                    folderName = $"R-{restaurantCounter++}";
                }
                else
                {
                    folderName = $"M-{menuCounter++}";
                }

                var folderPath = Path.Combine(extractPath, folderName);

                if (Directory.Exists(folderPath))
                {
                    var uploadedImage = Directory.GetFiles(folderPath).FirstOrDefault();

                    if (uploadedImage != null)
                    {
                        // ⬅ this needs ImagePath property on your models
                        item.ImageUrl = uploadedImage;
                    }
                }
            }

            TempData["msg"] = "Images uploaded and linked successfully!";
            return RedirectToAction("Catalog", "Items");
        }
    }
}
