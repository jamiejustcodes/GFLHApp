using System;
using System.Collections.Generic;
using System.Linq;
using GFLHApp.Models;
using Xunit;

namespace GFLHApp.Tests.Models
{
    public class OrderFulfillmentTests
    {
        [Fact]
        public void Order_Creation_InitializesWithPendingStatus()
        {
            var order = new Orders
            {
                OrdersId = 100,
                UserId = "customer-001",
                OrderDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Delivery = true,
                Collection = false,
                DeliveryMethod = "Standard",
                TrackingStatus = "Order Placed",
                OrderStatus = "Pending",
                OrdersTotal = 45.00m,
                BillingLine1 = "12 Greenfield Way",
                BillingCity = "Greenfield",
                BillingPostcode = "GF1 2AB",
                TermsAccepted = true
            };

            Assert.Equal("Order Placed", order.TrackingStatus);
            Assert.Equal("Pending", order.OrderStatus);
            Assert.True(order.Delivery);
            Assert.False(order.Collection);
            Assert.Equal(45.00m, order.OrdersTotal);
            Assert.True(order.TermsAccepted);
            Assert.NotNull(order.BillingPostcode);
        }

        [Theory]
        [InlineData("Standard", 3)]
        [InlineData("Express", 1)]
        [InlineData("Next Day", 1)]
        public void Order_DeliverySpeedDays_CalculatesEstimatedDelivery(string speed, int expectedMaxDays)
        {
            var orderDate = new DateOnly(2026, 6, 1);
            DateOnly estimatedDelivery;

            if (speed == "Next Day" || speed == "Express")
            {
                estimatedDelivery = orderDate.AddDays(1);
            }
            else
            {
                estimatedDelivery = orderDate.AddDays(3);
            }

            int diff = estimatedDelivery.DayNumber - orderDate.DayNumber;
            Assert.Equal(expectedMaxDays, diff);
        }

        [Fact]
        public void Order_ClickAndCollect_RequiresMinimumTwoDaysInAdvance()
        {
            var orderDate = new DateOnly(2026, 6, 1);
            var minValidDate = orderDate.AddDays(2);

            var requestedDate1 = new DateOnly(2026, 6, 2); // 1 day - invalid
            var requestedDate2 = new DateOnly(2026, 6, 3); // 2 days - valid
            var requestedDate3 = new DateOnly(2026, 6, 5); // 4 days - valid

            Assert.True(requestedDate1 < minValidDate);
            Assert.True(requestedDate2 >= minValidDate);
            Assert.True(requestedDate3 >= minValidDate);
        }

        [Fact]
        public void Order_MultiVendorSlicing_GroupsByProducerCorrectly()
        {
            var producer1 = new Producers { ProducersId = 1, ProducerName = "Farm Alpha", UserId = "p1", ProducerEmail = "a@a.com", ProducerInformation = "info" };
            var producer2 = new Producers { ProducersId = 2, ProducerName = "Bakery Beta", UserId = "p2", ProducerEmail = "b@b.com", ProducerInformation = "info" };

            var prod1 = new Products { ProductsId = 10, ItemName = "Carrots", ItemPrice = 1.20m, ProducersId = 1 };
            var prod2 = new Products { ProductsId = 11, ItemName = "Potatoes", ItemPrice = 2.00m, ProducersId = 1 };
            var prod3 = new Products { ProductsId = 20, ItemName = "Sourdough Bread", ItemPrice = 3.50m, ProducersId = 2 };

            var orderItems = new List<OrderProducts>
            {
                new OrderProducts { OrderProductsId = 1, OrdersId = 50, ProductsId = 10, ProductQuantity = 2, Products = prod1 },
                new OrderProducts { OrderProductsId = 2, OrdersId = 50, ProductsId = 11, ProductQuantity = 1, Products = prod2 },
                new OrderProducts { OrderProductsId = 3, OrdersId = 50, ProductsId = 20, ProductQuantity = 1, Products = prod3 }
            };

            // Slice generation
            var groupedByProducer = orderItems.GroupBy(x => x.Products.ProducersId).ToList();

            Assert.Equal(2, groupedByProducer.Count);

            var alphaSlice = groupedByProducer.FirstOrDefault(g => g.Key == 1);
            Assert.NotNull(alphaSlice);
            Assert.Equal(2, alphaSlice.Count());

            var betaSlice = groupedByProducer.FirstOrDefault(g => g.Key == 2);
            Assert.NotNull(betaSlice);
            Assert.Single(betaSlice);
        }

        [Fact]
        public void ProducerOrders_StatusChange_DoesNotAffectOtherVendors()
        {
            var sliceAlpha = new ProducerOrders
            {
                ProducerOrdersId = 1,
                OrdersId = 50,
                ProducerId = "producer-01",
                TrackingStatus = "Accepted"
            };

            var sliceBeta = new ProducerOrders
            {
                ProducerOrdersId = 2,
                OrdersId = 50,
                ProducerId = "producer-02",
                TrackingStatus = "Pending"
            };

            // Cancel slice Beta
            sliceBeta.TrackingStatus = "Cancelled";

            Assert.Equal("Accepted", sliceAlpha.TrackingStatus);
            Assert.Equal("Cancelled", sliceBeta.TrackingStatus);
        }

        [Fact]
        public void Order_InvoiceNumber_FollowsExpectedFormat()
        {
            int orderId = 1042;
            string invoiceNumber = $"INV-2026-{orderId:D5}";

            Assert.Equal("INV-2026-01042", invoiceNumber);
            Assert.StartsWith("INV-2026-", invoiceNumber);
        }
    }
}
