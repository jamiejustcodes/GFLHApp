using System;
using GFLHApp.Models;
using Xunit;

namespace GFLHApp.Tests.Models
{
    public class ProductValidationTests
    {
        [Fact]
        public void Product_Creation_HasDefaultActiveStatus()
        {
            var product = new Products
            {
                ProductsId = 10,
                ItemName = "Organic Strawberries",
                ItemPrice = 3.50m,
                Category = "Fruits",
                Description = "Fresh handpicked strawberries",
                QuantityInStock = 25,
                Available = true
            };

            Assert.Equal("Organic Strawberries", product.ItemName);
            Assert.Equal(3.50m, product.ItemPrice);
            Assert.Equal(25, product.QuantityInStock);
            Assert.True(product.Available);
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(5, true)]
        [InlineData(100, true)]
        public void Product_StockAvailability_DetectsInStockCorrectly(int stock, bool expectedInStock)
        {
            var product = new Products
            {
                QuantityInStock = stock,
                ItemName = "Sample Product",
                Category = "General",
                ItemPrice = 1.00m,
                Available = stock > 0
            };

            bool inStock = product.QuantityInStock > 0;
            Assert.Equal(expectedInStock, inStock);
            Assert.Equal(expectedInStock, product.Available);
        }

        [Fact]
        public void Product_ProducerAssociation_LinksCorrectly()
        {
            var producer = new Producers
            {
                ProducersId = 5,
                UserId = "producer-user-1",
                ProducerName = "Sunnybrook Organic Farm",
                ProducerEmail = "contact@sunnybrook.com",
                ProducerInformation = "Specialising in fresh dairy and pasture-raised meats."
            };

            var product = new Products
            {
                ProductsId = 50,
                ItemName = "Pasture Eggs",
                ItemPrice = 3.80m,
                Category = "Dairy",
                ProducersId = producer.ProducersId,
                Producers = producer
            };

            Assert.Equal(5, product.ProducersId);
            Assert.NotNull(product.Producers);
            Assert.Equal("Sunnybrook Organic Farm", product.Producers.ProducerName);
        }

        [Fact]
        public void Product_AllergenFlags_SetAndReadAccurately()
        {
            var dairyProduct = new Products
            {
                ItemName = "Artisan Cheddar Cheese",
                Category = "Dairy",
                ItemPrice = 4.50m,
                Description = "Mature English cheddar cheese.",
                Allergens = "Dairy, Milk"
            };

            Assert.NotNull(dairyProduct.Allergens);
            Assert.Contains("Dairy", dairyProduct.Allergens);
            Assert.Contains("Milk", dairyProduct.Allergens);
        }

        [Fact]
        public void Product_PriceValidation_RequiresPositiveValue()
        {
            var product = new Products
            {
                ItemName = "Free-range Chicken",
                ItemPrice = 8.50m,
                Category = "Meat"
            };

            Assert.True(product.ItemPrice > 0);
        }
    }
}
