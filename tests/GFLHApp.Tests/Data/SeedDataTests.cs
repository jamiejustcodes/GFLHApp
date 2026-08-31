using System.Linq;
using System.Threading.Tasks;
using GFLHApp.Models;
using GFLHApp.Tests.Helpers;
using Xunit;

namespace GFLHApp.Tests.Data
{
    public class SeedDataTests
    {
        [Fact]
        public async Task Database_CanAddAndQueryProducers()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();

            var producer = new Producers
            {
                ProducersId = 1,
                UserId = "producer-seed-01",
                ProducerName = "Green Valley Produce",
                ProducerEmail = "info@greenvalley.co.uk",
                ProducerInformation = "Family-run organic vegetable farm in Greenfield."
            };

            context.Producers.Add(producer);
            await context.SaveChangesAsync();

            var savedProducer = context.Producers.FirstOrDefault(p => p.ProducerName == "Green Valley Produce");
            Assert.NotNull(savedProducer);
            Assert.Equal("info@greenvalley.co.uk", savedProducer.ProducerEmail);
        }

        [Fact]
        public async Task Database_ProductProducerCascade_MaintainsIntegrity()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();

            var producer = new Producers
            {
                ProducersId = 2,
                UserId = "producer-seed-02",
                ProducerName = "Meadow Fresh Dairy",
                ProducerEmail = "orders@meadowfresh.co.uk",
                ProducerInformation = "Grass-fed cows milk, butter, and artisan yogurts."
            };

            context.Producers.Add(producer);
            context.Products.Add(new Products
            {
                ProductsId = 101,
                ItemName = "Organic Whole Milk 2L",
                ItemPrice = 2.40m,
                Category = "Dairy",
                Description = "Fresh pasteurized whole milk",
                ProducersId = producer.ProducersId
            });

            await context.SaveChangesAsync();

            var product = context.Products.FirstOrDefault(p => p.ProductsId == 101);
            Assert.NotNull(product);
            Assert.Equal(2, product.ProducersId);
        }

        [Fact]
        public async Task Database_BasketAndProducts_PersistsRelationships()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();

            var basket = new Basket
            {
                BasketId = 1,
                UserId = "test-user-basket"
            };

            var prod = new Products
            {
                ProductsId = 201,
                ItemName = "Fresh Strawberries",
                ItemPrice = 3.00m,
                Category = "Fruits",
                Description = "Local punnet"
            };

            var bp = new BasketProducts
            {
                BasketProductsId = 1,
                BasketId = 1,
                ProductsId = 201,
                ProductQuantity = 4,
                Basket = basket,
                Products = prod
            };

            context.Basket.Add(basket);
            context.Products.Add(prod);
            context.BasketProducts.Add(bp);
            await context.SaveChangesAsync();

            var savedBasketProduct = context.BasketProducts.FirstOrDefault(x => x.BasketProductsId == 1);
            Assert.NotNull(savedBasketProduct);
            Assert.Equal(4, savedBasketProduct.ProductQuantity);
            Assert.Equal(201, savedBasketProduct.ProductsId);
        }
    }
}
