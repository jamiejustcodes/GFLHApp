using GFLHApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Producer portal controller handling catalogue management, slice fulfillment,
    /// item cancellation/restocking, and multi-producer order status aggregation.
    /// </summary>
    [Authorize(Roles = "Producer,Developer")]
    public class ProducerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProducerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Displays producer metrics, catalogue inventory status, and pending/recent order slices.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers.FirstOrDefaultAsync(p => p.UserId == userId);

            if (producer == null) return NotFound();

            var products = await _context.Products
                .Where(p => p.ProducersId == producer.ProducersId)
                .ToListAsync();

            var producerOrders = await _context.ProducerOrders
                .Where(x => x.ProducerId == userId)
                .Include(x => x.Orders)
                .Include(x => x.OrderProducts)
                    .ThenInclude(x => x.Products)
                .OrderByDescending(x => x.Orders.OrderDate)
                .ToListAsync();

            var now = DateTime.UtcNow;

            ViewBag.ProducerName = producer.ProducerName;
            ViewBag.TotalProducts = products.Count;
            ViewBag.LowStockCount = products.Count(p => p.QuantityInStock <= 5 && p.Available);
            ViewBag.TotalStock = products.Sum(p => p.QuantityInStock);
            ViewBag.PendingCount = producerOrders.Count(o => o.TrackingStatus == "Pending");
            ViewBag.TotalRevenue = producerOrders.Where(o => o.TrackingStatus == "Accepted").Sum(o => o.ProducerSubtotal);
            ViewBag.ThisMonthRevenue = producerOrders
                .Where(o => o.TrackingStatus == "Accepted"
                         && o.Orders.OrderDate.Month == now.Month
                         && o.Orders.OrderDate.Year == now.Year)
                .Sum(o => o.ProducerSubtotal);
            ViewBag.ProducerOrders = producerOrders;

            return View(products);
        }

        // GET: ProducerDashboard/AllOrders
        public async Task<IActionResult> AllOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var producerOrders = await _context.ProducerOrders
                .Where(x => x.ProducerId == userId)
                .Include(x => x.Orders)
                .Include(x => x.OrderProducts)
                    .ThenInclude(x => x.Products)
                .OrderByDescending(x => x.Orders.OrderDate)
                .ToListAsync();

            return View(producerOrders);
        }

        /// <summary>
        /// Toggles product availability asynchronously from the producer dashboard.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers.FirstOrDefaultAsync(p => p.UserId == userId);
            if (producer == null) return Json(new { success = false });

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductsId == id && p.ProducersId == producer.ProducersId);
            if (product == null) return Json(new { success = false });

            product.Available = !product.Available;
            await _context.SaveChangesAsync();
            return Json(new { success = true, available = product.Available });
        }

        /// <summary>
        /// Recalculates the composite status of a parent order based on the fulfillment states of all its producer slices.
        /// </summary>
        private async Task RecalculateOrderStatus(int ordersId)
        {
            var allSlices = await _context.ProducerOrders
                .Where(x => x.OrdersId == ordersId)
                .ToListAsync();

            var order = await _context.Orders.FindAsync(ordersId);
            if (order == null) return;

            bool allCancelled = allSlices.All(x => x.TrackingStatus == "Cancelled");
            bool allAccepted = allSlices.All(x => x.TrackingStatus == "Accepted");
            bool anyCancelled = allSlices.Any(x => x.TrackingStatus == "Cancelled");
            bool anyAccepted = allSlices.Any(x => x.TrackingStatus == "Accepted");

            if (allCancelled)
                order.OrderStatus = "Cancelled";
            else if (allAccepted)
                order.OrderStatus = "Accepted";
            else if (anyCancelled && anyAccepted)
                order.OrderStatus = "Partially Complete";
            else
                order.OrderStatus = "Pending";

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Cancels all items in a producer's slice, restocks the inventory, and updates the parent order total.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("CancelProducerOrder")]
        public async Task<IActionResult> CancelProducerOrderPost(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var producerOrder = await _context.ProducerOrders
                .Where(x => x.ProducerOrdersId == id && x.ProducerId == userId)
                .Include(x => x.Orders)
                .Include(x => x.OrderProducts)
                    .ThenInclude(x => x.Products)
                .FirstOrDefaultAsync();

            if (producerOrder == null) return NotFound();
            if (producerOrder.TrackingStatus == "Cancelled") return RedirectToAction("Index");

            foreach (var item in producerOrder.OrderProducts)
            {
                item.Products.QuantityInStock += item.ProductQuantity;
                producerOrder.Orders.OrdersTotal -= item.Products.ItemPrice * item.ProductQuantity;
            }

            producerOrder.ProducerSubtotal = 0;
            producerOrder.TrackingStatus = "Cancelled";
            _context.OrderProducts.RemoveRange(producerOrder.OrderProducts);

            await _context.SaveChangesAsync();
            await RecalculateOrderStatus(producerOrder.OrdersId);

            return RedirectToAction("Index");
        }

        // GET: ProducerDashboard/CancelProducerOrder/5
        public async Task<IActionResult> CancelProducerOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var producerOrder = await _context.ProducerOrders
                .Where(x => x.ProducerOrdersId == id && x.ProducerId == userId)
                .Include(x => x.Orders)
                .Include(x => x.OrderProducts)
                    .ThenInclude(x => x.Products)
                .FirstOrDefaultAsync();

            if (producerOrder == null)
            {
                return NotFound();
            }

            if (producerOrder.TrackingStatus == "Cancelled")
            {
                return RedirectToAction("Index");
            }

            return View(producerOrder);
        }

        /// <summary>
        /// Cancels and restocks a single order item line, adjusting producer subtotal and parent order total.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrderItem(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orderProduct = await _context.OrderProducts
                .Where(x => x.OrderProductsId == id)
                .Include(x => x.Products)
                .Include(x => x.ProducerOrders)
                    .ThenInclude(x => x.Orders)
                .FirstOrDefaultAsync();

            if (orderProduct == null || orderProduct.ProducerOrders.ProducerId != userId)
                return NotFound();

            // Restock cancelled product
            orderProduct.Products.QuantityInStock += orderProduct.ProductQuantity;

            // Deduct from producer subtotal and parent order total
            var lineTotal = orderProduct.Products.ItemPrice * orderProduct.ProductQuantity;
            orderProduct.ProducerOrders.ProducerSubtotal -= lineTotal;
            orderProduct.ProducerOrders.Orders.OrdersTotal -= lineTotal;

            // Update producer slice status if no lines remain
            var remainingItems = await _context.OrderProducts
                .CountAsync(x => x.ProducerOrdersId == orderProduct.ProducerOrdersId
                              && x.OrderProductsId != id);

            if (remainingItems == 0)
                orderProduct.ProducerOrders.TrackingStatus = "Cancelled";

            int ordersId = orderProduct.ProducerOrders.OrdersId;
            _context.OrderProducts.Remove(orderProduct);
            await _context.SaveChangesAsync();

            await RecalculateOrderStatus(ordersId);

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Marks a producer slice as accepted and cascades order status recalculation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptProducerOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var producerOrder = await _context.ProducerOrders
                .Include(x => x.Orders)
                .FirstOrDefaultAsync(x => x.ProducerOrdersId == id && x.ProducerId == userId);

            if (producerOrder == null)
                return NotFound();

            if (producerOrder.TrackingStatus == "Pending")
            {
                producerOrder.TrackingStatus = "Accepted";
                await _context.SaveChangesAsync();
                await RecalculateOrderStatus(producerOrder.OrdersId);
            }

            return RedirectToAction("Index");
        }
    }
}

