using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GFLHApp.Data;
using GFLHApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Manages product catalogue browsing, item details, Natasha's Law allergen tracking,
    /// and producer stock/inventory creation and updates.
    /// </summary>
    public class ProductsController : Controller
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const long MaxImageUploadBytes = 5 * 1024 * 1024;

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        /// <summary>
        /// Displays products; filters to own products if authenticated as Producer, otherwise lists full catalogue.
        /// </summary>
        public async Task<IActionResult> Index(int? producerId)
        {
            if (User.IsInRole("Producer"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                    return Unauthorized();

                var producer = await _context.Producers.FirstOrDefaultAsync(p => p.UserId == userId);

                if (producer == null)
                    return NotFound();

                var producerProducts = await _context.Products
                    .Where(p => p.ProducersId == producer.ProducersId)
                    .Include(p => p.Producers)
                    .ToListAsync();

                return View(producerProducts);
            }

            var query = _context.Products.Include(p => p.Producers).AsQueryable();

            if (producerId.HasValue)
                query = query.Where(p => p.ProducersId == producerId.Value);

            return View(await query.ToListAsync());
        }


        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Producers)
                .FirstOrDefaultAsync(m => m.ProductsId == id);

            if (products == null)
            {
                return NotFound();
            }

            var allOthers = await _context.Products
                .Where(p => p.ProducersId == products.ProducersId && p.ProductsId != id)
                .Include(p => p.Producers)
                .ToListAsync();

            var rng = new Random();
            ViewBag.OtherProducts = allOthers.OrderBy(_ => rng.Next()).Take(3).ToList();

            return View(products);
        }

        // GET: Products/Create
        [Authorize(Roles = "Producer,Developer")]
        public IActionResult Create()
        {
            PopulateExistingImages();
            return View(new Products { Available = true });
        }

        // POST: Products/Create
        [HttpPost]
        [Authorize(Roles = "Producer,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemName,ItemPrice,ImagePath,QuantityInStock,Available,Category,Description,Allergens")] Products products, IFormFile? imageUpload)
        {
            var producer = await GetCurrentProducerAsync();
            if (producer == null)
            {
                return Unauthorized();
            }

            ModelState.Remove(nameof(Products.Producers));
            products.ProducersId = producer.ProducersId;
            products.ImagePath = NormalizeImagePath(products.ImagePath);
            products.ItemName = products.ItemName?.Trim() ?? string.Empty;
            products.Category = products.Category?.Trim() ?? string.Empty;
            products.Description = products.Description?.Trim() ?? string.Empty;
            products.Allergens = NormalizeAllergens(products.Allergens);

            ValidateProduct(products);
            ValidateImageUpload(imageUpload);

            if (!ModelState.IsValid)
            {
                PopulateExistingImages();
                return View(products);
            }

            if (imageUpload != null)
            {
                products.ImagePath = await SaveProductImageAsync(imageUpload);
            }

            _context.Products.Add(products);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Producer,Developer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await GetCurrentProducerAsync();
            if (producer == null)
            {
                return Unauthorized();
            }

            var products = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductsId == id && p.ProducersId == producer.ProducersId);

            if (products == null)
            {
                return NotFound();
            }

            PopulateExistingImages();
            return View(products);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [Authorize(Roles = "Producer,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductsId,ItemName,ItemPrice,ImagePath,QuantityInStock,Available,Category,Description,Allergens")] Products products, IFormFile? imageUpload, string? existingImagePath)
        {
            if (id != products.ProductsId)
            {
                return NotFound();
            }

            var producer = await GetCurrentProducerAsync();
            if (producer == null)
            {
                return Unauthorized();
            }

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductsId == id && p.ProducersId == producer.ProducersId);

            if (existingProduct == null)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Products.Producers));
            products.ProducersId = producer.ProducersId;
            products.ImagePath = NormalizeImagePath(products.ImagePath);
            products.ItemName = products.ItemName?.Trim() ?? string.Empty;
            products.Category = products.Category?.Trim() ?? string.Empty;
            products.Description = products.Description?.Trim() ?? string.Empty;
            products.Allergens = NormalizeAllergens(products.Allergens);

            ValidateProduct(products);
            ValidateImageUpload(imageUpload);

            if (!ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(products.ImagePath))
                {
                    products.ImagePath = existingProduct.ImagePath ?? existingImagePath;
                }

                PopulateExistingImages();
                return View(products);
            }

            existingProduct.ItemName = products.ItemName;
            existingProduct.ItemPrice = products.ItemPrice;
            existingProduct.QuantityInStock = products.QuantityInStock;
            existingProduct.Available = products.Available;
            existingProduct.Category = products.Category;
            existingProduct.Description = products.Description;
            existingProduct.Allergens = products.Allergens;

            if (imageUpload != null)
            {
                existingProduct.ImagePath = await SaveProductImageAsync(imageUpload);
            }
            else if (!string.IsNullOrWhiteSpace(products.ImagePath))
            {
                existingProduct.ImagePath = products.ImagePath;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Producer,Developer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producer = await GetCurrentProducerAsync();
            if (producer == null)
            {
                return Unauthorized();
            }

            var products = await _context.Products
                .Include(p => p.Producers)
                .FirstOrDefaultAsync(m => m.ProductsId == id && m.ProducersId == producer.ProducersId);

            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Producer,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producer = await GetCurrentProducerAsync();
            if (producer == null)
            {
                return Unauthorized();
            }

            var products = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductsId == id && p.ProducersId == producer.ProducersId);

            if (products != null)
            {
                _context.Products.Remove(products);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<Producers?> GetCurrentProducerAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _context.Producers.FirstOrDefaultAsync(p => p.UserId == userId);
        }

        private void PopulateExistingImages()
        {
            var imagesRoot = Path.Combine(_environment.WebRootPath, "images");
            var imagePaths = Directory.Exists(imagesRoot)
                ? Directory.EnumerateFiles(imagesRoot, "*.*", SearchOption.AllDirectories)
                    .Where(path => AllowedImageExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
                    .Select(path => "/" + Path.GetRelativePath(_environment.WebRootPath, path).Replace('\\', '/'))
                    .OrderBy(path => path)
                    .ToList()
                : [];

            ViewBag.ExistingImages = new SelectList(imagePaths);
        }

        private void ValidateProduct(Products products)
        {
            if (string.IsNullOrWhiteSpace(products.ItemName))
            {
                ModelState.AddModelError(nameof(products.ItemName), "Enter a product name.");
            }

            if (products.ItemPrice <= 0)
            {
                ModelState.AddModelError(nameof(products.ItemPrice), "Price must be greater than 0.");
            }

            if (products.QuantityInStock < 0)
            {
                ModelState.AddModelError(nameof(products.QuantityInStock), "Stock quantity cannot be negative.");
            }

            if (string.IsNullOrWhiteSpace(products.Category))
            {
                ModelState.AddModelError(nameof(products.Category), "Select a category.");
            }

            if (string.IsNullOrWhiteSpace(products.Description))
            {
                ModelState.AddModelError(nameof(products.Description), "Enter a description.");
            }

            if (!string.IsNullOrWhiteSpace(products.ImagePath)
                && !products.ImagePath.StartsWith("/")
                && !Uri.IsWellFormedUriString(products.ImagePath, UriKind.Absolute))
            {
                ModelState.AddModelError(nameof(products.ImagePath), "Use a relative web path like /images/products/example.jpg or a full URL.");
            }
        }

        private void ValidateImageUpload(IFormFile? imageUpload)
        {
            if (imageUpload == null || imageUpload.Length == 0)
            {
                return;
            }

            var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError("imageUpload", "Upload a JPG, PNG, or WebP image.");
            }

            if (imageUpload.Length > MaxImageUploadBytes)
            {
                ModelState.AddModelError("imageUpload", "Image files must be smaller than 5 MB.");
            }
        }

        private async Task<string> SaveProductImageAsync(IFormFile imageUpload)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            var fileName = $"product-{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageUpload.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }

        private static string? NormalizeImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            return imagePath.Trim();
        }

        private static string? NormalizeAllergens(string? allergens)
        {
            if (string.IsNullOrWhiteSpace(allergens))
            {
                return null;
            }

            var normalized = allergens
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return string.Join(", ", normalized);
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.ProductsId == id);
        }
    }
}
