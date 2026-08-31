using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GFLHApp.Data;
using GFLHApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Manages line items within shopping baskets, supporting standard and AJAX add-to-cart operations.
    /// </summary>
    [Authorize(Roles = "Standard,Developer")]
    public class BasketProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BasketProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BasketProducts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.BasketProducts
                .Include(b => b.Basket)
                .Include(b => b.Products);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: BasketProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basketProducts = await _context.BasketProducts
                .Include(b => b.Basket)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BasketProductsId == id);

            if (basketProducts == null)
            {
                return NotFound();
            }

            return View(basketProducts);
        }

        // GET: BasketProducts/Create
        public IActionResult Create()
        {
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId");
            ViewData["ProductsId"] = new SelectList(_context.Products, "ProductsId", "ProductsId");
            return View();
        }

        // POST: BasketProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ProductsId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductsId == ProductsId);
            if (product == null)
            {
                return NotFound();
            }

            if (!product.Available)
            {
                TempData["Error"] = "This product is currently unavailable and cannot be added to your basket.";
                return RedirectToAction("Index", "Products");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Retrieve or create the active basket for the current user
            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.UserId == userId && x.Status);
            if (basket == null)
            {
                basket = new Basket
                {
                    Status = true,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Basket.Add(basket);
                await _context.SaveChangesAsync();
            }

            // Increment quantity if item is already present, otherwise add a new line item
            var basketProduct = await _context.BasketProducts
                .FirstOrDefaultAsync(bp => bp.BasketId == basket.BasketId && bp.ProductsId == ProductsId);

            if (basketProduct != null)
            {
                basketProduct.ProductQuantity++;
            }
            else
            {
                basketProduct = new BasketProducts
                {
                    BasketId = basket.BasketId,
                    ProductsId = ProductsId,
                    ProductQuantity = 1
                };
                _context.BasketProducts.Add(basketProduct);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Baskets");
        }

        /// <summary>
        /// Returns the total number of items in the user's active basket for the live UI badge.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Json(new { count = 0 });

            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.UserId == userId && x.Status);
            if (basket == null) return Json(new { count = 0 });

            var count = await _context.BasketProducts
                .Where(bp => bp.BasketId == basket.BasketId)
                .SumAsync(bp => bp.ProductQuantity);

            return Json(new { count });
        }

        /// <summary>
        /// Asynchronously adds a specified quantity of a product to the user's active basket without page reload.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAjax(int ProductsId, int Quantity = 1)
        {
            if (Quantity < 1) Quantity = 1;
            if (Quantity > 99) Quantity = 99;

            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductsId == ProductsId);
            if (product == null)
                return Json(new { success = false, message = "Product not found." });

            if (!product.Available)
                return Json(new { success = false, message = "This product is currently unavailable." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Json(new { success = false, message = "Please log in to add items to your basket." });

            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.UserId == userId && x.Status);
            if (basket == null)
            {
                basket = new Basket { Status = true, UserId = userId, CreatedAt = DateTime.UtcNow };
                _context.Basket.Add(basket);
                await _context.SaveChangesAsync();
            }

            var basketProduct = await _context.BasketProducts
                .FirstOrDefaultAsync(bp => bp.BasketId == basket.BasketId && bp.ProductsId == ProductsId);

            if (basketProduct != null)
                basketProduct.ProductQuantity += Quantity;
            else
                _context.BasketProducts.Add(new BasketProducts
                {
                    BasketId = basket.BasketId,
                    ProductsId = ProductsId,
                    ProductQuantity = Quantity
                });

            await _context.SaveChangesAsync();

            var basketCount = await _context.BasketProducts
                .Where(bp => bp.BasketId == basket.BasketId)
                .SumAsync(bp => bp.ProductQuantity);

            return Json(new { success = true, basketCount, itemName = product.ItemName, quantity = Quantity });
        }

        // GET: BasketProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basketProducts = await _context.BasketProducts.FindAsync(id);
            if (basketProducts == null)
            {
                return NotFound();
            }
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId", basketProducts.BasketId);
            ViewData["ProductsId"] = new SelectList(_context.Products, "ProductsId", "ProductsId", basketProducts.ProductsId);
            return View(basketProducts);
        }

        // POST: BasketProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BasketProductsId,BasketId,ProductsId,ProductQuantity")] BasketProducts basketProducts)
        {
            if (id != basketProducts.BasketProductsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(basketProducts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BasketProductsExists(basketProducts.BasketProductsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId", basketProducts.BasketId);
            ViewData["ProductsId"] = new SelectList(_context.Products, "ProductsId", "ProductsId", basketProducts.ProductsId);
            return View(basketProducts);
        }

        // GET: BasketProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basketProducts = await _context.BasketProducts
                .Include(b => b.Basket)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BasketProductsId == id);
            if (basketProducts == null)
            {
                return NotFound();
            }

            return View(basketProducts);
        }

        /// <summary>
        /// Removes an item line from the current authenticated user's active shopping cart.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Standard,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromBasket(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var basketProduct = await _context.BasketProducts
                .Include(bp => bp.Basket)
                .FirstOrDefaultAsync(bp => bp.BasketProductsId == id
                    && bp.Basket != null
                    && bp.Basket.UserId == userId
                    && bp.Basket.Status);

            if (basketProduct == null)
            {
                return NotFound();
            }

            _context.BasketProducts.Remove(basketProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Baskets");
        }

        // POST: BasketProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var basketProducts = await _context.BasketProducts.FindAsync(id);
            if (basketProducts != null)
            {
                _context.BasketProducts.Remove(basketProducts);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BasketProductsExists(int id)
        {
            return _context.BasketProducts.Any(e => e.BasketProductsId == id);
        }
    }
}

