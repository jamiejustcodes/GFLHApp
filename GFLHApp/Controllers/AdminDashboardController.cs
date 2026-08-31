using GFLHApp.Data;
using GFLHApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Administrative dashboard providing business analytics, global order management,
    /// catalogue moderation, role management, and system-wide statistics.
    /// </summary>
    [Authorize(Roles = "Admin,Developer")]
    public class AdminDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminDashboardController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ── Overview ─────────────────────────────────────────────────────

        /// <summary>
        /// Displays platform KPIs, recent orders, best-selling products, and 6-month revenue trends.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var now = DateTime.UtcNow;

            var orders = await _context.Orders.ToListAsync();
            var products = await _context.Products.ToListAsync();
            var producers = await _context.Producers.ToListAsync();

            // High-level summary metrics
            ViewBag.TotalUsers = _userManager.Users.Count();
            ViewBag.TotalOrders = orders.Count;
            ViewBag.TotalProducts = products.Count;
            ViewBag.TotalProducers = producers.Count;
            ViewBag.TotalRevenue = orders.Sum(o => o.OrdersTotal);
            ViewBag.PendingOrders = orders.Count(o => o.OrderStatus == "Pending");
            ViewBag.LowStock = products.Count(p => p.QuantityInStock <= 5 && p.Available);
            ViewBag.MonthRevenue = orders
                .Where(o => o.OrderDate.Month == now.Month && o.OrderDate.Year == now.Year)
                .Sum(o => o.OrdersTotal);

            // Recent order feed
            ViewBag.RecentOrders = await _context.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.Products)
                .OrderByDescending(o => o.OrderDate)
                .Take(6)
                .ToListAsync();

            // Top 5 best-selling products by total units ordered
            var topProductIds = await _context.OrderProducts
                .GroupBy(op => op.ProductsId)
                .Select(g => new { ProductsId = g.Key, TotalOrdered = g.Sum(op => op.ProductQuantity) })
                .OrderByDescending(x => x.TotalOrdered)
                .Take(5)
                .ToListAsync();

            var pIds = topProductIds.Select(x => x.ProductsId).ToList();
            var pNames = await _context.Products
                .Where(p => pIds.Contains(p.ProductsId))
                .ToDictionaryAsync(p => p.ProductsId, p => p.ItemName);

            ViewBag.TopProducts = topProductIds
                .Where(x => pNames.ContainsKey(x.ProductsId))
                .Select(x => new { Name = pNames[x.ProductsId], x.TotalOrdered })
                .ToList();

            // 6-month historical monthly revenue breakdown
            var sixMonthsAgo = DateOnly.FromDateTime(now.AddMonths(-5));
            ViewBag.MonthlyRevenue = orders
                .Where(o => o.OrderDate >= sixMonthsAgo)
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new {
                    Label = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                    Revenue = g.Sum(o => o.OrdersTotal),
                    g.Key.Year,
                    g.Key.Month
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // Order status distribution
            ViewBag.StatusBreakdown = orders
                .GroupBy(o => o.OrderStatus ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return View();
        }

        // ── Orders ───────────────────────────────────────────────────────

        /// <summary>
        /// Lists all platform orders with multi-filter search, status filters, and sorting.
        /// </summary>
        public async Task<IActionResult> Orders(string search = "", string orderStatus = "", string trackingStatus = "", string sort = "newest")
        {
            var query = _context.Orders
                .Include(o => o.OrderProducts).ThenInclude(op => op.Products)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                if (int.TryParse(s, out int orderId))
                    query = query.Where(o => o.OrdersId == orderId);
                else
                    query = query.Where(o => o.UserId.ToLower().Contains(s) ||
                                             o.BillingCity.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(orderStatus))
                query = query.Where(o => o.OrderStatus == orderStatus);

            if (!string.IsNullOrWhiteSpace(trackingStatus))
                query = query.Where(o => o.TrackingStatus == trackingStatus);

            query = sort switch
            {
                "oldest" => query.OrderBy(o => o.OrderDate),
                "total_asc" => query.OrderBy(o => o.OrdersTotal),
                "total_desc" => query.OrderByDescending(o => o.OrdersTotal),
                _ => query.OrderByDescending(o => o.OrderDate)
            };

            var orders = await query.ToListAsync();

            ViewBag.Search = search;
            ViewBag.OrderStatus = orderStatus;
            ViewBag.TrackingStatus = trackingStatus;
            ViewBag.Sort = sort;

            return View(orders);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTrackingStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null) 
            { 
                order.TrackingStatus = status; 
                await _context.SaveChangesAsync(); 
            }
            return RedirectToAction(nameof(Orders));
        }

        // ── Products ─────────────────────────────────────────────────────

        /// <summary>
        /// Platform-wide product catalogue management and stock level monitoring.
        /// </summary>
        public async Task<IActionResult> Products(string search = "", string category = "", string availability = "", string sort = "name")
        {
            var query = _context.Products.Include(p => p.Producers).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p => p.ItemName.ToLower().Contains(s) ||
                                         p.Category.ToLower().Contains(s) ||
                                         p.Producers.ProducerName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            query = availability switch
            {
                "available" => query.Where(p => p.Available),
                "unavailable" => query.Where(p => !p.Available),
                "lowstock" => query.Where(p => p.QuantityInStock <= 5 && p.Available),
                _ => query
            };

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.ItemPrice),
                "price_desc" => query.OrderByDescending(p => p.ItemPrice),
                "stock_asc" => query.OrderBy(p => p.QuantityInStock),
                "stock_desc" => query.OrderByDescending(p => p.QuantityInStock),
                _ => query.OrderBy(p => p.ItemName)
            };

            var products = await query.ToListAsync();
            var categories = await _context.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Availability = availability;
            ViewBag.Sort = sort;
            ViewBag.Categories = categories;

            return View(products);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductAvailability(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return Json(new { success = false });
            product.Available = !product.Available;
            await _context.SaveChangesAsync();
            return Json(new { success = true, available = product.Available });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null) 
            { 
                _context.Products.Remove(product); 
                await _context.SaveChangesAsync(); 
            }
            TempData["AdminSuccess"] = "Product deleted.";
            return RedirectToAction(nameof(Products));
        }

        // ── Producers ────────────────────────────────────────────────────

        /// <summary>
        /// Lists all registered producers along with active product counts and orders.
        /// </summary>
        public async Task<IActionResult> Producers(string search = "")
        {
            var query = _context.Producers
                .Include(p => p.Products)
                .Include(p => p.ProducerOrders)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p => p.ProducerName.ToLower().Contains(s) ||
                                         p.ProducerEmail.ToLower().Contains(s));
            }

            var producers = await query.OrderBy(p => p.ProducerName).ToListAsync();
            ViewBag.Search = search;
            return View(producers);
        }

        // ── Users ────────────────────────────────────────────────────────

        /// <summary>
        /// User administration interface for viewing roles, order counts, and assigning/revoking access.
        /// </summary>
        public async Task<IActionResult> Users(string search = "", string role = "")
        {
            var allUsers = _userManager.Users.OrderBy(u => u.Email).ToList();
            var rows = new List<AdminUserRow>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role)) continue;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    if (!(user.Email?.ToLower().Contains(s) == true ||
                          user.UserName?.ToLower().Contains(s) == true)) continue;
                }

                var orderCount = await _context.Orders.CountAsync(o => o.UserId == user.Id);
                rows.Add(new AdminUserRow(user, roles, orderCount));
            }

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList();

            return View(rows);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            await _userManager.AddToRoleAsync(user, roleName);
            TempData["AdminSuccess"] = $"Role '{roleName}' assigned to {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            await _userManager.RemoveFromRoleAsync(user, roleName);
            TempData["AdminSuccess"] = $"Role '{roleName}' removed from {user.Email}.";
            return RedirectToAction(nameof(Users));
        }

        // ── Settings ─────────────────────────────────────────────────────

        /// <summary>
        /// System settings and configuration summary.
        /// </summary>
        public async Task<IActionResult> Settings()
        {
            ViewBag.UserCount = _userManager.Users.Count();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            ViewBag.ProductCount = await _context.Products.CountAsync();
            ViewBag.ProducerCount = await _context.Producers.CountAsync();
            ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => (decimal?)o.OrdersTotal) ?? 0m;
            ViewBag.AllRoles = _roleManager.Roles.Select(r => r.Name).OrderBy(n => n).ToList();
            ViewBag.ActiveBaskets = await _context.Basket.CountAsync(b => b.Status);
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName) && !await _roleManager.RoleExistsAsync(roleName.Trim()))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
                TempData["AdminSuccess"] = $"Role '{roleName.Trim()}' created.";
            }
            return RedirectToAction(nameof(Settings));
        }
    }

    public record AdminUserRow(IdentityUser User, IList<string> Roles, int OrderCount);
}

