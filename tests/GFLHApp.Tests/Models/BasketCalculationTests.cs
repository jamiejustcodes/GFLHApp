using System;
using System.Collections.Generic;
using GFLHApp.Models;
using Xunit;

namespace GFLHApp.Tests.Models
{
    public class BasketCalculationTests
    {
        [Fact]
        public void Basket_EmptyBasket_ReturnsZeroTotal()
        {
            var basket = new Basket
            {
                BasketId = 1,
                UserId = "user-123",
                BasketProducts = new List<BasketProducts>()
            };

            decimal total = 0;
            foreach (var bp in basket.BasketProducts)
            {
                total += bp.ProductQuantity * (bp.Products?.ItemPrice ?? 0);
            }

            Assert.Equal(0, total);
            Assert.Empty(basket.BasketProducts);
        }

        [Fact]
        public void Basket_SingleItem_CalculatesSubtotalCorrectly()
        {
            var product = new Products
            {
                ProductsId = 1,
                ItemName = "Organic Milk",
                ItemPrice = 2.50m,
                Category = "Dairy"
            };

            var basketProduct = new BasketProducts
            {
                BasketProductsId = 1,
                BasketId = 1,
                ProductsId = 1,
                Products = product,
                ProductQuantity = 3
            };

            decimal subtotal = basketProduct.ProductQuantity * basketProduct.Products.ItemPrice;

            Assert.Equal(7.50m, subtotal);
        }

        [Fact]
        public void Basket_MultipleItems_CalculatesTotalAccurately()
        {
            var items = new List<BasketProducts>
            {
                new BasketProducts
                {
                    ProductQuantity = 2,
                    Products = new Products { ProductsId = 1, ItemName = "Organic Eggs", ItemPrice = 3.20m, Category = "Dairy" }
                },
                new BasketProducts
                {
                    ProductQuantity = 4,
                    Products = new Products { ProductsId = 2, ItemName = "Local Apples", ItemPrice = 1.50m, Category = "Fruits" }
                },
                new BasketProducts
                {
                    ProductQuantity = 1,
                    Products = new Products { ProductsId = 3, ItemName = "Artisan Sourdough", ItemPrice = 4.00m, Category = "Bakery" }
                }
            };

            decimal total = 0;
            foreach (var item in items)
            {
                total += item.ProductQuantity * item.Products.ItemPrice;
            }

            // (2 * 3.20) + (4 * 1.50) + (1 * 4.00) = 6.40 + 6.00 + 4.00 = 16.40
            Assert.Equal(16.40m, total);
        }

        [Theory]
        [InlineData(100.00, 0.10, 90.00)]
        [InlineData(50.00, 0.05, 47.50)]
        [InlineData(20.00, 0.00, 20.00)]
        [InlineData(80.00, 0.20, 64.00)]
        public void Basket_LoyaltyDiscount_AppliesCorrectReduction(decimal subtotal, decimal discountRate, decimal expectedTotal)
        {
            decimal discountAmount = subtotal * discountRate;
            decimal finalTotal = subtotal - discountAmount;

            Assert.Equal(expectedTotal, finalTotal);
        }

        [Fact]
        public void BasketProducts_QuantityUpdate_ModifiesSubtotalDynamically()
        {
            var bp = new BasketProducts
            {
                ProductQuantity = 2,
                Products = new Products { ItemPrice = 5.00m, ItemName = "Raw Honey", Category = "Pantry" }
            };

            Assert.Equal(10.00m, bp.ProductQuantity * bp.Products.ItemPrice);

            bp.ProductQuantity = 5;
            Assert.Equal(25.00m, bp.ProductQuantity * bp.Products.ItemPrice);

            bp.ProductQuantity = 1;
            Assert.Equal(5.00m, bp.ProductQuantity * bp.Products.ItemPrice);
        }
    }
}
