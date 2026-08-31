using GFLHApp.Data;
using GFLHApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Handles public producer profile exploration and administrative registration/editing of producer accounts.
    /// </summary>
    public class ProducersController : Controller
    {
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const long MaxImageUploadBytes = 5 * 1024 * 1024;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _environment;

        public ProducersController(ApplicationDbContext context,
                                   UserManager<IdentityUser> userManager,
                                   RoleManager<IdentityRole> roleManager,
                                   IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _environment = environment;
        }

        // GET: Producers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Producers.ToListAsync());
        }

        // GET: Producers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producers = await _context.Producers
                .Include(p => p.Products)
                .FirstOrDefaultAsync(m => m.ProducersId == id);

            if (producers == null) return NotFound();

            return View(producers);
        }

        [Authorize(Roles = "Developer,Admin")]
        // GET: Producers/Create
        public IActionResult Create()
        {
            PopulateExistingProducerImages();
            return View();
        }

        /// <summary>
        /// Registers a new producer profile while simultaneously provisioning an associated Identity user with the Producer role.
        /// </summary>
        [Authorize(Roles = "Developer,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ProducersId,ProducerName,ProducerEmail,ProducerInformation,ImagePath,IsVATRegistered,VATNumber")]
            Producers producers,
            IFormFile? imageUpload,
            string AccountPassword)
        {
            // UserId and navigation collections are generated post-validation
            ModelState.Remove("UserId");
            ModelState.Remove("ProducerOrders");
            NormalizeProducer(producers);
            ValidateProducerFields(producers);
            ValidateImageUpload(imageUpload);

            if (!ModelState.IsValid)
            {
                PopulateExistingProducerImages();
                return View(producers);
            }

            // Uniqueness validation
            bool emailInUse = await _context.Producers
                .AnyAsync(p => p.ProducerEmail.ToLower() == producers.ProducerEmail.ToLower());
            if (emailInUse)
                ModelState.AddModelError("ProducerEmail", "This email address is already registered to a producer.");

            bool nameInUse = await _context.Producers
                .AnyAsync(p => p.ProducerName.ToLower() == producers.ProducerName.ToLower());
            if (nameInUse)
                ModelState.AddModelError("ProducerName", "This producer name is already taken.");

            if (producers.IsVATRegistered && !string.IsNullOrWhiteSpace(producers.VATNumber))
            {
                bool vatInUse = await _context.Producers
                    .AnyAsync(p => p.VATNumber == producers.VATNumber);
                if (vatInUse)
                    ModelState.AddModelError("VATNumber", "This VAT number is already registered to another producer.");
            }

            // Credential validation
            if (string.IsNullOrWhiteSpace(AccountPassword))
                ModelState.AddModelError("AccountPassword", "A password is required.");

            if (!ModelState.IsValid)
            {
                PopulateExistingProducerImages();
                return View(producers);
            }

            // Create and configure the ASP.NET Core Identity user account
            var identityUser = new IdentityUser
            {
                UserName = producers.ProducerEmail,
                Email = producers.ProducerEmail,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(identityUser, AccountPassword);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("AccountPassword", error.Description);
                PopulateExistingProducerImages();
                return View(producers);
            }

            // Ensure Producer role is configured and assign it
            if (!await _roleManager.RoleExistsAsync("Producer"))
                await _roleManager.CreateAsync(new IdentityRole("Producer"));

            await _userManager.AddToRoleAsync(identityUser, "Producer");

            // Attach user ID and save uploaded profile image
            producers.UserId = identityUser.Id;
            if (imageUpload != null)
            {
                producers.ImagePath = await SaveProducerImageAsync(imageUpload);
            }

            _context.Add(producers);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Developer,Admin")]
        // GET: Producers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producers = await _context.Producers.FindAsync(id);
            if (producers == null) return NotFound();

            PopulateExistingProducerImages();
            return View(producers);
        }

        [Authorize(Roles = "Developer,Admin")]
        // POST: Producers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("ProducersId,UserId,ProducerName,ProducerEmail,ProducerInformation,ImagePath,IsVATRegistered,VATNumber")]
            Producers producers,
            IFormFile? imageUpload,
            string? existingImagePath)
        {
            if (id != producers.ProducersId) return NotFound();

            var existingProducer = await _context.Producers.FirstOrDefaultAsync(p => p.ProducersId == id);
            if (existingProducer == null) return NotFound();

            NormalizeProducer(producers);
            ValidateProducerFields(producers);
            ValidateImageUpload(imageUpload);

            // Uniqueness checks excluding the current record
            bool emailInUse = await _context.Producers
                .AnyAsync(p => p.ProducerEmail.ToLower() == producers.ProducerEmail.ToLower()
                            && p.ProducersId != id);
            if (emailInUse)
                ModelState.AddModelError("ProducerEmail", "This email address is already registered to another producer.");

            bool nameInUse = await _context.Producers
                .AnyAsync(p => p.ProducerName.ToLower() == producers.ProducerName.ToLower()
                            && p.ProducersId != id);
            if (nameInUse)
                ModelState.AddModelError("ProducerName", "This producer name is already taken.");

            if (producers.IsVATRegistered && !string.IsNullOrWhiteSpace(producers.VATNumber))
            {
                bool vatInUse = await _context.Producers
                    .AnyAsync(p => p.VATNumber == producers.VATNumber
                               && p.ProducersId != id);
                if (vatInUse)
                    ModelState.AddModelError("VATNumber", "This VAT number is already registered to another producer.");
            }

            if (!ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(producers.ImagePath))
                {
                    producers.ImagePath = existingProducer.ImagePath ?? existingImagePath;
                }

                PopulateExistingProducerImages();
                return View(producers);
            }

            try
            {
                existingProducer.UserId = producers.UserId;
                existingProducer.ProducerName = producers.ProducerName;
                existingProducer.ProducerEmail = producers.ProducerEmail;
                existingProducer.ProducerInformation = producers.ProducerInformation;
                existingProducer.IsVATRegistered = producers.IsVATRegistered;
                existingProducer.VATNumber = producers.IsVATRegistered ? producers.VATNumber : null;

                if (imageUpload != null)
                {
                    existingProducer.ImagePath = await SaveProducerImageAsync(imageUpload);
                }
                else if (!string.IsNullOrWhiteSpace(producers.ImagePath))
                {
                    existingProducer.ImagePath = producers.ImagePath;
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProducersExists(producers.ProducersId)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Developer,Admin")]
        // GET: Producers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producers = await _context.Producers
                .FirstOrDefaultAsync(m => m.ProducersId == id);

            if (producers == null) return NotFound();

            return View(producers);
        }

        [Authorize(Roles = "Developer,Admin")]
        // POST: Producers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producers = await _context.Producers.FindAsync(id);
            if (producers != null)
                _context.Producers.Remove(producers);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProducersExists(int id)
        {
            return _context.Producers.Any(e => e.ProducersId == id);
        }

        private void PopulateExistingProducerImages()
        {
            var imagesRoot = Path.Combine(_environment.WebRootPath, "images", "producers");
            var imagePaths = Directory.Exists(imagesRoot)
                ? Directory.EnumerateFiles(imagesRoot, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(path => AllowedImageExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
                    .Select(path => "/images/producers/" + Path.GetFileName(path))
                    .OrderBy(path => path)
                    .ToList()
                : [];

            ViewBag.ExistingImages = new SelectList(imagePaths);
        }

        private void NormalizeProducer(Producers producers)
        {
            producers.ProducerName = producers.ProducerName?.Trim() ?? string.Empty;
            producers.ProducerEmail = producers.ProducerEmail?.Trim() ?? string.Empty;
            producers.ProducerInformation = producers.ProducerInformation?.Trim() ?? string.Empty;
            producers.ImagePath = NormalizeImagePath(producers.ImagePath);
            producers.VATNumber = producers.IsVATRegistered
                ? producers.VATNumber?.Trim().ToUpperInvariant()
                : null;
        }

        private void ValidateProducerFields(Producers producers)
        {
            if (string.IsNullOrWhiteSpace(producers.ProducerName))
            {
                ModelState.AddModelError(nameof(producers.ProducerName), "Producer name is required.");
            }
            else if (producers.ProducerName.Length < 3 || producers.ProducerName.Length > 100)
            {
                ModelState.AddModelError(nameof(producers.ProducerName), "Producer name must be between 3 and 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(producers.ProducerEmail))
            {
                ModelState.AddModelError(nameof(producers.ProducerEmail), "Email address is required.");
            }
            else if (producers.ProducerEmail.Length > 150 || !producers.ProducerEmail.Contains('@'))
            {
                ModelState.AddModelError(nameof(producers.ProducerEmail), "Enter a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(producers.ProducerInformation))
            {
                ModelState.AddModelError(nameof(producers.ProducerInformation), "Producer information is required.");
            }
            else if (producers.ProducerInformation.Length < 10 || producers.ProducerInformation.Length > 500)
            {
                ModelState.AddModelError(nameof(producers.ProducerInformation), "Producer information must be between 10 and 500 characters.");
            }

            if (producers.IsVATRegistered)
            {
                if (string.IsNullOrWhiteSpace(producers.VATNumber))
                {
                    ModelState.AddModelError(nameof(producers.VATNumber), "VAT number is required if VAT registered.");
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(producers.VATNumber, "^GB[0-9]{9}$"))
                {
                    ModelState.AddModelError(nameof(producers.VATNumber), "VAT number must start with GB followed by exactly 9 digits.");
                }
            }

            if (!string.IsNullOrWhiteSpace(producers.ImagePath)
                && !producers.ImagePath.StartsWith("/")
                && !Uri.IsWellFormedUriString(producers.ImagePath, UriKind.Absolute))
            {
                ModelState.AddModelError(nameof(producers.ImagePath), "Use a relative web path like /images/producers/example.jpg or a full URL.");
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

        private async Task<string> SaveProducerImageAsync(IFormFile imageUpload)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "producers");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageUpload.FileName).ToLowerInvariant();
            var fileName = $"producer-{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await imageUpload.CopyToAsync(stream);

            return $"/images/producers/{fileName}";
        }

        private static string? NormalizeImagePath(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            return imagePath.Trim();
        }
    }
}
