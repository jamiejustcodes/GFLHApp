using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GFLHApp.Data;
using GFLHApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Manages the customer's active basket session, line item aggregation,
    /// and business discount calculations (loyalty discount & health bundle promo).
    /// </summary>
    [Authorize(Roles = "Standard,Developer")]
    public class BasketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BasketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Baskets
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Retrieve or initialize the active basket for the current user
            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.UserId == userId && x.Status);
            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = userId,
                    Status = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Basket.Add(basket);
                await _context.SaveChangesAsync();
            }

            // Retrieve basket line items and compute subtotal
            var basketProducts = await _context.BasketProducts
                .Where(bp => bp.BasketId == basket.BasketId)
                .Include(bp => bp.Basket)
                .Include(bp => bp.Products)
                .ToListAsync();

            decimal subtotal = 0m;
            foreach (var basketProduct in basketProducts)
            {
                var productTotal = basketProduct.Products.ItemPrice * basketProduct.ProductQuantity;
                subtotal += productTotal;
            }

            // Check previous order history for loyalty reward
            var orderCount = await _context.Orders.CountAsync(o => o.UserId == userId);

            // Promotional discount rules:
            // 1. Health bundle promo: 10% off if basket contains broccoli, carrot, AND apple
            var productNames = basketProducts.Select(x => x.Products.ItemName.ToLower()).ToList();
            bool hasHealthBundle = productNames.Contains("broccoli") &&
                                   productNames.Contains("carrot") &&
                                   productNames.Contains("apple");

            decimal discount = 0m;

            // 2. Loyalty discount: 15% off on every 5th order (4th prior order means this order is #5)
            if (orderCount % 5 == 4)
            {
                discount = subtotal * 0.15m;
            }
            else if (hasHealthBundle)
            {
                discount = subtotal * 0.10m;
            }

            decimal total = subtotal - discount;

            ViewBag.Subtotal = subtotal;
            ViewBag.Discount = discount;
            ViewBag.Total = total;
            ViewBag.OrderCount = orderCount;
            ViewBag.HasHealthBundle = hasHealthBundle;

            return View(basketProducts);
        }

        // GET: Baskets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basket = await _context.Basket
                .FirstOrDefaultAsync(m => m.BasketId == id);
            if (basket == null)
            {
                return NotFound();
            }

            return View(basket);
        }

        // GET: Baskets/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Baskets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BasketId,UserId,Status,CreatedAt")] Basket basket)
        {
            if (ModelState.IsValid)
            {
                _context.Add(basket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(basket);
        }

        // GET: Baskets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basket = await _context.Basket.FindAsync(id);
            if (basket == null)
            {
                return NotFound();
            }
            return View(basket);
        }

        // POST: Baskets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BasketId,UserId,Status,CreatedAt")] Basket basket)
        {
            if (id != basket.BasketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(basket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BasketExists(basket.BasketId))
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
            return View(basket);
        }

        // GET: Baskets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basket = await _context.Basket
                .FirstOrDefaultAsync(m => m.BasketId == id);
            if (basket == null)
            {
                return NotFound();
            }

            return View(basket);
        }

        // POST: Baskets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var basket = await _context.Basket.FindAsync(id);
            if (basket != null)
            {
                _context.Basket.Remove(basket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BasketExists(int id)
        {
            return _context.Basket.Any(e => e.BasketId == id);
        }
    }
}