using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Administrative controller for viewing and moderating individual producer order slices.
    /// </summary>
    [Authorize(Roles = "Admin,Developer")]
    public class ProducerOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProducerOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ProducerOrders
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ProducerOrders.Include(p => p.Orders);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ProducerOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producerOrders = await _context.ProducerOrders
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(m => m.ProducerOrdersId == id);
            if (producerOrders == null)
            {
                return NotFound();
            }

            return View(producerOrders);
        }

        // GET: ProducerOrders/Create
        public IActionResult Create()
        {
            ViewData["OrdersId"] = new SelectList(_context.Orders, "OrdersId", "OrdersId");
            return View();
        }

        // POST: ProducerOrders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProducerOrdersId,OrdersId,ProducerId,ProducerSubtotal,TrackingStatus")] ProducerOrders producerOrders)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producerOrders);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrdersId"] = new SelectList(_context.Orders, "OrdersId", "OrdersId", producerOrders.OrdersId);
            return View(producerOrders);
        }

        // GET: ProducerOrders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producerOrders = await _context.ProducerOrders.FindAsync(id);
            if (producerOrders == null)
            {
                return NotFound();
            }
            ViewData["OrdersId"] = new SelectList(_context.Orders, "OrdersId", "OrdersId", producerOrders.OrdersId);
            return View(producerOrders);
        }

        // POST: ProducerOrders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProducerOrdersId,OrdersId,ProducerId,ProducerSubtotal,TrackingStatus")] ProducerOrders producerOrders)
        {
            if (id != producerOrders.ProducerOrdersId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producerOrders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProducerOrdersExists(producerOrders.ProducerOrdersId))
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
            ViewData["OrdersId"] = new SelectList(_context.Orders, "OrdersId", "OrdersId", producerOrders.OrdersId);
            return View(producerOrders);
        }

        // GET: ProducerOrders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producerOrders = await _context.ProducerOrders
                .Include(p => p.Orders)
                .FirstOrDefaultAsync(m => m.ProducerOrdersId == id);
            if (producerOrders == null)
            {
                return NotFound();
            }

            return View(producerOrders);
        }

        // POST: ProducerOrders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producerOrders = await _context.ProducerOrders.FindAsync(id);
            if (producerOrders != null)
            {
                _context.ProducerOrders.Remove(producerOrders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProducerOrdersExists(int id)
        {
            return _context.ProducerOrders.Any(e => e.ProducerOrdersId == id);
        }
    }
}

