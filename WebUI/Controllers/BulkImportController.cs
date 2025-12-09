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
        // memory = temporary, db = final storage
        private readonly ItemsRepository _memoryRepo;
        private readonly ItemsRepository _dbRepo;
        private readonly ImportItemFactory _factory;

        public BulkImportController(
            [FromKeyedServices("memory")] ItemsRepository memoryRepo,
            [FromKeyedServices("db")] ItemsRepository dbRepo,
            ImportItemFactory factory)
        {
            _memoryRepo = memoryRepo;
            _dbRepo = dbRepo;
            _factory = factory;
        }

        // STEP 1 → Show JSON upload page
        [HttpGet]
        public IActionResult BulkImport()
        {
            return View();
        }

        // STEP 2 → Upload JSON + generate ZIP with default images
        [HttpPost]
        public IActionResult BulkImport(IFormFile jsonFile)
        {
            if (jsonFile == null || jsonFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid JSON file.");
                return View();
            }

            // Read JSON
            string json;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                json = reader.ReadToEnd();
            }

            // Parse JSON → instances
            var items = _factory.Create(json);

            // Save in memory (AA2.3)
            _memoryRepo.Save(items);

            // Build temp folder for ZIP
            var tempFolder = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid()}");
            Directory.CreateDirectory(tempFolder);

            int restaurantCounter = 1;
            int menuCounter = 1;

            // Default image in wwwroot
            var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "default.jpg");

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

            // Download ZIP
            var bytes = System.IO.File.ReadAllBytes(zipPath);
            return new FileContentResult(bytes, "application/zip")
            {
                FileDownloadName = zipName
            };
        }

        // STEP 3 → Commit: upload edited ZIP, save images in wwwroot, link & save to DB

        [HttpGet]
        public IActionResult Commit()
        {
            return View("UploadImages");
        }

        [HttpPost]
        public IActionResult Commit(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a valid ZIP file.");
                return View("UploadImages");
            }

            // 1) Extract ZIP to temp
            var extractPath = Path.Combine(Path.GetTempPath(), $"commit-{Guid.NewGuid()}");
            Directory.CreateDirectory(extractPath);

            var zipPath = Path.Combine(extractPath, "uploaded.zip");
            using (var stream = new FileStream(zipPath, FileMode.Create))
            {
                zipFile.CopyTo(stream);
            }

            ZipFile.ExtractToDirectory(zipPath, extractPath);

            // 2) Get pending items from memory repo
            var items = _memoryRepo.Get();

            int restaurantCounter = 1;
            int menuCounter = 1;

            // 3) Prepare wwwroot image folders
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var restaurantsRoot = Path.Combine(webRoot, "images", "restaurants");
            var menuItemsRoot = Path.Combine(webRoot, "images", "menuitems");

            Directory.CreateDirectory(restaurantsRoot);
            Directory.CreateDirectory(menuItemsRoot);

            foreach (var item in items)
            {
                string folderName;
                bool isRestaurant;

                if (item.GetType().Name.ToLower().Contains("restaurant"))
                {
                    folderName = $"R-{restaurantCounter++}";
                    isRestaurant = true;
                }
                else
                {
                    folderName = $"M-{menuCounter++}";
                    isRestaurant = false;
                }

                var folderPath = Path.Combine(extractPath, folderName);
                if (!Directory.Exists(folderPath))
                    continue;

                var uploadedImage = Directory.GetFiles(folderPath).FirstOrDefault();
                if (uploadedImage == null)
                    continue;

                var extension = Path.GetExtension(uploadedImage);
                var uniqueName = $"{Guid.NewGuid()}{extension}";
                var targetFolder = isRestaurant ? restaurantsRoot : menuItemsRoot;
                var targetPath = Path.Combine(targetFolder, uniqueName);

                System.IO.File.Copy(uploadedImage, targetPath, true);

                var relativePath = $"/images/{(isRestaurant ? "restaurants" : "menuitems")}/{uniqueName}";
                item.ImageUrl = relativePath;
            }

            // 6) Save everything into DB using db repo
            _dbRepo.Save(items);

            TempData["msg"] = "Items committed to database with images!";
            return RedirectToAction("Catalog", "Items");
        }
    }
}
