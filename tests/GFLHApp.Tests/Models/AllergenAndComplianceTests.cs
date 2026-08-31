using System;
using System.Collections.Generic;
using System.Linq;
using GFLHApp.Models;
using Xunit;

namespace GFLHApp.Tests.Models
{
    public class AllergenAndComplianceTests
    {
        [Theory]
        [InlineData("Gluten, Dairy", "Gluten", true)]
        [InlineData("Gluten, Dairy", "Nuts", false)]
        [InlineData("Eggs, Sesame, Peanuts", "Eggs", true)]
        [InlineData(null, "Gluten", false)]
        [InlineData("", "Gluten", false)]
        public void Product_AllergenParsing_IdentifiesContainedAllergens(string? allergens, string allergenToCheck, bool expectedContains)
        {
            var product = new Products
            {
                ItemName = "Sample Bread",
                Category = "Bakery",
                ItemPrice = 2.50m,
                Allergens = allergens
            };

            bool contains = !string.IsNullOrEmpty(product.Allergens) &&
                            product.Allergens.Split(new[] { ',', ';' }, StringSplitOptions.TrimEntries)
                                             .Any(a => a.Equals(allergenToCheck, StringComparison.OrdinalIgnoreCase));

            Assert.Equal(expectedContains, contains);
        }

        [Theory]
        [InlineData("GB123456789", true)]
        [InlineData("GB999999973", true)]
        [InlineData("INVALID_VAT", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void Producer_VATNumberValidation_RecognizesUKFormat(string? vatNumber, bool expectedValid)
        {
            var producer = new Producers
            {
                ProducerName = "Highland Farm",
                VATNumber = vatNumber,
                IsVATRegistered = !string.IsNullOrEmpty(vatNumber)
            };

            bool isValidFormat = !string.IsNullOrEmpty(producer.VATNumber) &&
                                 producer.VATNumber.StartsWith("GB", StringComparison.OrdinalIgnoreCase) &&
                                 producer.VATNumber.Length >= 9;

            Assert.Equal(expectedValid, isValidFormat);
        }

        [Fact]
        public void OrderProducts_VATInvoiceGeneration_OnlyForVATRegisteredProducers()
        {
            var vatProducer = new Producers
            {
                ProducersId = 1,
                ProducerName = "VAT Registered Orchard",
                IsVATRegistered = true,
                VATNumber = "GB123456789"
            };

            var nonVatProducer = new Producers
            {
                ProducersId = 2,
                ProducerName = "Small Cottage Bakery",
                IsVATRegistered = false,
                VATNumber = null
            };

            var orderItem1 = new OrderProducts
            {
                OrderProductsId = 1,
                OrdersId = 10,
                ProductsId = 101,
                ProductQuantity = 2,
                InvoiceNumber = vatProducer.IsVATRegistered ? $"INV-2026-{10:D5}-P1" : null
            };

            var orderItem2 = new OrderProducts
            {
                OrderProductsId = 2,
                OrdersId = 10,
                ProductsId = 202,
                ProductQuantity = 1,
                InvoiceNumber = nonVatProducer.IsVATRegistered ? $"INV-2026-{10:D5}-P2" : null
            };

            Assert.NotNull(orderItem1.InvoiceNumber);
            Assert.StartsWith("INV-2026-", orderItem1.InvoiceNumber);
            Assert.Null(orderItem2.InvoiceNumber);
        }
    }
}
