using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GFLHApp.Data;
using GFLHApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GFLHApp.Controllers
{
    /// <summary>
    /// Handles checkout processing, order placement, multi-producer slicing,
    /// tax invoice generation, address validation, and customer order history.
    /// </summary>
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Automatically advances tracking statuses based on elapsed calendar days and delivery tiers.
        /// </summary>
        private async Task SyncDeliveryStatuses(List<Orders> orders)
        {
            foreach (var order in orders.Where(o => o.Delivery))
            {
                double days = (DateTime.Now - order.OrderDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
                int preparingDays = order.DeliveryMethod switch
                {
                    "Next Day" => 1,
                    "Standard" => 3,
                    "Economy" => 7,
                    _ => 3
                };

                string newStatus;
                if (order.DeliveryConfirmed || days >= preparingDays + 3)
                    newStatus = "Delivered";
                else if (days >= preparingDays)
                    newStatus = "Awaiting Confirmation";
                else
                    newStatus = "Preparing Delivery";

                if (order.TrackingStatus != newStatus)
                    order.TrackingStatus = newStatus;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Allows customers to confirm receipt of their order delivery.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Standard,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.OrdersId == id && x.UserId == userId);

            if (order == null) return NotFound();

            order.DeliveryConfirmed = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Displays order history tailored to role (Admin sees all, Producer sees their orders, Standard sees own orders).
        /// </summary>
        [Authorize(Roles = "Standard,Developer,Admin")]
        public async Task<IActionResult> Index(List<Orders> orders)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (User.IsInRole("Admin"))
            {
                var allOrders = await _context.Orders
                    .Include(x => x.OrderProducts)
                        .ThenInclude(x => x.Products)
                    .ToListAsync();
                return View(allOrders);
            }
            else if (User.IsInRole("Producer"))
            {
                var producerOrders = await _context.ProducerOrders
                    .Where(x => x.ProducerId == userId)
                    .Include(x => x.Orders)
                    .Include(x => x.OrderProducts)
                        .ThenInclude(x => x.Products)
                    .ToListAsync();

                return View(producerOrders.Select(op => op.Orders).Distinct().ToList());
            }
            else
            {
                var userOrders = await _context.Orders
                    .Where(x => x.UserId == userId)
                    .Include(x => x.OrderProducts)
                        .ThenInclude(x => x.Products)
                    .ToListAsync();
                return View(userOrders);
            }
        }

        // GET: Orders/Details/5  
        [Authorize(Roles = "Standard,Developer")]
        public async Task<IActionResult> Details(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Products)
                .FirstOrDefaultAsync(o => o.OrdersId == id && o.UserId == userId);

            if (order == null)
            {
                return Unauthorized();
            }

            return View(order);
        }

        // GET: Orders/Create
        [Authorize(Roles = "Standard,Developer")]
        public async Task<IActionResult> Create(int basketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var basketProducts = await _context.BasketProducts
                .Where(x => x.BasketId == basketId)
                .Include(x => x.Products)
                    .ThenInclude(x => x.Producers)
                .ToListAsync();

            decimal subtotal = 0m;
            foreach (var item in basketProducts)
            {
                subtotal += item.Products.ItemPrice * item.ProductQuantity;
            }

            var orderCount = await _context.Orders.CountAsync(x => x.UserId == userId);

            // Promotional discount eligibility
            var productNames = basketProducts.Select(x => x.Products.ItemName.ToLower()).ToList();
            bool hasHealthBundle = productNames.Contains("broccoli") &&
                                   productNames.Contains("carrot") &&
                                   productNames.Contains("apple");

            decimal discount = 0m;
            if (orderCount % 5 == 4)
            {
                discount = subtotal * 0.15m;
            }
            else if (hasHealthBundle)
            {
                discount = subtotal * 0.10m;
            }

            decimal discountedSubtotal = subtotal - discount;

            ViewBag.BasketId = basketId;
            ViewBag.Subtotal = discountedSubtotal;
            ViewBag.HasFreeDelivery = orderCount % 3 == 2;
            ViewBag.HasHealthBundle = hasHealthBundle;
            ViewBag.BasketProducts = basketProducts;
            ViewBag.DeliveryCosts = new Dictionary<string, decimal>
            {
                { "Next Day",  5.99m },
                { "Standard",  2.99m },
                { "Economy",   0.99m }
            };

            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [Authorize(Roles = "Standard,Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrdersId,Delivery,Collection,DeliveryMethod,DateOfCollection,BillingLine1,BillingLine2,BillingCity,BillingPostcode,DeliveryLine1,DeliveryLine2,DeliveryCity,DeliveryPostcode,TermsAccepted")] Orders orders, int basketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                ViewBag.BasketId = basketId;
                return View(orders);
            }

            // Explicit terms acceptance check
            if (!orders.TermsAccepted)
            {
                ModelState.AddModelError("TermsAccepted", "You must accept the terms and conditions to place an order.");
            }

            // Bind metadata values and remove computed properties from ModelState
            orders.UserId = userId;
            orders.OrderDate = DateOnly.FromDateTime(DateTime.Today);
            orders.TrackingStatus = "Pending";

            ModelState.Remove("InvoiceNumber");
            ModelState.Remove("UserId");
            ModelState.Remove("OrderDate");
            ModelState.Remove("TrackingStatus");

            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.BasketId == basketId && x.Status);
            if (basket == null)
            {
                return NotFound();
            }

            var basketProducts = await _context.BasketProducts
                .Where(x => x.BasketId == basketId)
                .Include(x => x.Products)
                    .ThenInclude(x => x.Producers)
                .ToListAsync();

            if (!basketProducts.Any())
            {
                ModelState.AddModelError("", "Your basket is currently empty.");
                ViewBag.BasketId = basketId;
                ViewBag.BasketProducts = basketProducts;
                return View(orders);
            }

            // Subtotal calculation
            decimal subtotal = 0.00m;
            foreach (var basketProduct in basketProducts)
            {
                subtotal += basketProduct.Products.ItemPrice * basketProduct.ProductQuantity;
            }

            var orderCouut = await _context.Orders.CountAsync(x => x.UserId == userId);

            // Promotional discount check
            var basketProductNames = basketProducts.Select(x => x.Products.ItemName.ToLower()).ToList();
            bool hasHealthBundle = basketProductNames.Contains("broccoli") &&
                                   basketProductNames.Contains("carrot") &&
                                   basketProductNames.Contains("apple");

            // Shipping cost calculation with loyalty perk (free shipping every 3rd order)
            decimal deliveryCost = 0m;
            if (orders.Delivery)
            {
                if (orderCouut % 3 == 2)
                {
                    deliveryCost = 0m;
                }
                else
                {
                    deliveryCost = orders.DeliveryMethod switch
                    {
                        "Next Day" => 5.99m,
                        "Standard" => 2.99m,
                        "Economy" => 0.99m,
                        _ => 0m
                    };
                }
            }

            // Discount calculation: 15% loyalty on 5th order OR 10% health bundle promo
            decimal discount = 0m;
            if (orderCouut % 5 == 4)
            {
                discount = subtotal * 0.15m;
            }
            else if (hasHealthBundle)
            {
                discount = subtotal * 0.10m;
            }

            orders.OrdersTotal = (subtotal - discount) + deliveryCost;

            ViewBag.BasketId = basketId;
            ViewBag.Subtotal = subtotal;
            ViewBag.HasFreeDelivery = orderCouut % 3 == 2;
            ViewBag.HasHealthBundle = hasHealthBundle;
            ViewBag.DeliveryCosts = new Dictionary<string, decimal>
            {
                { "Next Day",  5.99m },
                { "Standard",  2.99m },
                { "Economy",   0.99m }
            };

            ModelState.Remove("OrdersTotal");
            ModelState.Remove("BillingLine2");
            ModelState.Remove("DeliveryLine2");

            // Address format regex validation (UK standard address characters and postcodes)
            var addressRegex = new Regex(@"^[a-zA-Z0-9 #\-,\.]{1,40}$");
            var postcodeRegex = new Regex(@"^[A-Z]{1,2}[0-9][0-9A-Z]?\s?[0-9][A-Z]{2}$", RegexOptions.IgnoreCase);

            // Billing address validation
            if (string.IsNullOrWhiteSpace(orders.BillingLine1))
            {
                ModelState.AddModelError("BillingLine1", "Please enter the first line of your billing address.");
            }
            else if (orders.BillingLine1.Length > 40)
            {
                ModelState.AddModelError("BillingLine1", "Address line 1 must not exceed 40 characters.");
            }
            else if (!addressRegex.IsMatch(orders.BillingLine1))
            {
                ModelState.AddModelError("BillingLine1", "Address line 1 can only contain letters, numbers, spaces, and the following: # - , .");
            }

            if (!string.IsNullOrWhiteSpace(orders.BillingLine2))
            {
                if (orders.BillingLine2.Length > 40)
                {
                    ModelState.AddModelError("BillingLine2", "Address line 2 must not exceed 40 characters.");
                }
                else if (!addressRegex.IsMatch(orders.BillingLine2))
                {
                    ModelState.AddModelError("BillingLine2", "Address line 2 can only contain letters, numbers, spaces, and the following: # - , .");
                }
            }

            if (string.IsNullOrWhiteSpace(orders.BillingCity))
            {
                ModelState.AddModelError("BillingCity", "Please enter your billing city.");
            }
            else if (orders.BillingCity.Length > 40)
            {
                ModelState.AddModelError("BillingCity", "City must not exceed 40 characters.");
            }
            else if (!addressRegex.IsMatch(orders.BillingCity))
            {
                ModelState.AddModelError("BillingCity", "City can only contain letters, numbers, spaces, and the following: # - , .");
            }

            if (string.IsNullOrWhiteSpace(orders.BillingPostcode))
            {
                ModelState.AddModelError("BillingPostcode", "Please enter your billing postcode.");
            }
            else if (!postcodeRegex.IsMatch(orders.BillingPostcode.Trim()))
            {
                ModelState.AddModelError("BillingPostcode", "Please enter a valid UK postcode (e.g. B1 1BB or SW1A 2AA).");
            }

            // Delivery address validation
            if (orders.Delivery)
            {
                if (string.IsNullOrWhiteSpace(orders.DeliveryLine1))
                {
                    ModelState.AddModelError("DeliveryLine1", "Please enter the first line of your delivery address.");
                }
                else if (orders.DeliveryLine1.Length > 40)
                {
                    ModelState.AddModelError("DeliveryLine1", "Address line 1 must not exceed 40 characters.");
                }
                else if (!addressRegex.IsMatch(orders.DeliveryLine1))
                {
                    ModelState.AddModelError("DeliveryLine1", "Address line 1 can only contain letters, numbers, spaces, and the following: # - , .");
                }

                if (!string.IsNullOrWhiteSpace(orders.DeliveryLine2))
                {
                    if (orders.DeliveryLine2.Length > 40)
                    {
                        ModelState.AddModelError("DeliveryLine2", "Address line 2 must not exceed 40 characters.");
                    }
                    else if (!addressRegex.IsMatch(orders.DeliveryLine2))
                    {
                        ModelState.AddModelError("DeliveryLine2", "Address line 2 can only contain letters, numbers, spaces, and the following: # - , .");
                    }
                }

                if (string.IsNullOrWhiteSpace(orders.DeliveryCity))
                {
                    ModelState.AddModelError("DeliveryCity", "Please enter your delivery city.");
                }
                else if (orders.DeliveryCity.Length > 40)
                {
                    ModelState.AddModelError("DeliveryCity", "City must not exceed 40 characters.");
                }
                else if (!addressRegex.IsMatch(orders.DeliveryCity))
                {
                    ModelState.AddModelError("DeliveryCity", "City can only contain letters, numbers, spaces, and the following: # - , .");
                }

                if (string.IsNullOrWhiteSpace(orders.DeliveryPostcode))
                {
                    ModelState.AddModelError("DeliveryPostcode", "Please enter your delivery postcode.");
                }
                else if (!postcodeRegex.IsMatch(orders.DeliveryPostcode.Trim()))
                {
                    ModelState.AddModelError("DeliveryPostcode", "Please enter a valid UK postcode (e.g. B1 1BB or SW1A 2AA).");
                }
            }

            // Fulfillment method selection rules
            if (!orders.Collection && !orders.Delivery)
            {
                ModelState.AddModelError("Delivery", "Please select either Delivery or Collection.");
            }

            if (orders.Collection)
            {
                ModelState.Remove("DeliveryMethod");

                if (orders.DateOfCollection == null)
                {
                    ModelState.AddModelError("DateOfCollection", "Please provide a date for collection.");
                }
                else
                {
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var earliestCollectionDate = today.AddDays(2);
                    var latestCollectionDate = today.AddDays(14);

                    if (orders.DateOfCollection.Value < today)
                    {
                        ModelState.AddModelError("DateOfCollection", "Collection date must be in the present or future.");
                    }
                    else if (orders.DateOfCollection.Value < earliestCollectionDate)
                    {
                        ModelState.AddModelError("DateOfCollection", "Collection must be at least 2 days from today.");
                    }
                    else if (orders.DateOfCollection.Value > latestCollectionDate)
                    {
                        ModelState.AddModelError("DateOfCollection", "Collection date must be within the next 14 days from today.");
                    }
                }
            }

            if (orders.Delivery)
            {
                ModelState.Remove("CollectionDate");

                if (string.IsNullOrWhiteSpace(orders.DeliveryMethod))
                {
                    ModelState.AddModelError("DeliveryMethod", "Please select a delivery method.");
                }

                if (string.IsNullOrWhiteSpace(orders.DeliveryLine1))
                {
                    ModelState.AddModelError("DeliveryLine1", "Please enter the first line of your delivery address.");
                }

                if (string.IsNullOrWhiteSpace(orders.DeliveryCity))
                {
                    ModelState.AddModelError("DeliveryCity", "Please enter your delivery city.");
                }

                if (string.IsNullOrWhiteSpace(orders.DeliveryPostcode))
                {
                    ModelState.AddModelError("DeliveryPostcode", "Please enter your delivery postcode.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BasketId = basketId;
                ViewBag.BasketProducts = basketProducts;
                return View(orders);
            }

            // Inventory stock check
            foreach (var basketProduct in basketProducts)
            {
                if (basketProduct.Products.QuantityInStock < basketProduct.ProductQuantity)
                {
                    ModelState.AddModelError("", $"The stock is too low for {basketProduct.Products.ItemName}");
                    ViewBag.BasketId = basketId;
                    ViewBag.BasketProducts = basketProducts;
                    return View(orders);
                }
            }

            // Save parent order record
            _context.Orders.Add(orders);
            await _context.SaveChangesAsync();

            // Deconstruct basket into producer order slices for each selling vendor
            var groupedByProducer = basketProducts.GroupBy(x => x.Products.Producers.UserId);

            foreach (var producerGroup in groupedByProducer)
            {
                decimal producerSubtotal = 0m;
                foreach (var item in producerGroup)
                {
                    producerSubtotal += item.Products.ItemPrice * item.ProductQuantity;
                }

                var producerOrder = new ProducerOrders
                {
                    OrdersId = orders.OrdersId,
                    ProducerId = producerGroup.Key,
                    ProducerSubtotal = producerSubtotal,
                    TrackingStatus = "Pending"
                };

                _context.ProducerOrders.Add(producerOrder);
                await _context.SaveChangesAsync();

                // Create order item lines linked to the producer slice and generate VAT invoices where required
                foreach (var basketProduct in producerGroup)
                {
                    var orderProduct = new OrderProducts
                    {
                        OrdersId = orders.OrdersId,
                        ProducerOrdersId = producerOrder.ProducerOrdersId,
                        ProductsId = basketProduct.ProductsId,
                        ProductQuantity = basketProduct.ProductQuantity,
                        InvoiceNumber = (basketProduct.Products.Producers != null && basketProduct.Products.Producers.IsVATRegistered)
                            ? $"INV-{orders.OrderDate:yyyyMMdd}-{orders.OrdersId:D6}-{basketProduct.Products.Producers.ProducersId}"
                            : null
                    };

                    _context.OrderProducts.Add(orderProduct);
                    basketProduct.Products.QuantityInStock -= basketProduct.ProductQuantity;
                }
            }

            // Finalize checkout session and close basket
            basket.Status = false;
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", "Orders", new { id = orders.OrdersId });
        }

        // GET: Orders/Edit/5
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [Authorize(Roles = "Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrdersId,UserId,OrderDate,DeliveryMethod,Delivery,Collection,OrdersTotal,TrackingStatus,DateOfCollection,BillingLine1,BillingLine2,BillingCity,BillingPostcode,DeliveryLine1,DeliveryLine2,DeliveryCity,DeliveryPostcode")] Orders orders)
        {
            if (id != orders.OrdersId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.OrdersId))
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

            return View(orders);
        }

        // GET: Orders/Delete/5
        [Authorize(Roles = "Developer")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrdersId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Developer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders != null)
            {
                _context.Orders.Remove(orders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.OrdersId == id);
        }

        /// <summary>
        /// Renders order confirmation receipt, tax breakdowns, and fulfillment details.
        /// </summary>
        [Authorize(Roles = "Standard,Developer")]
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var order = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Products)
                    .ThenInclude(p => p.Producers)
                .FirstOrDefaultAsync(o => o.OrdersId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}

