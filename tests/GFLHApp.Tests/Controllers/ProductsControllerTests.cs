using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GFLHApp.Controllers;
using GFLHApp.Models;
using GFLHApp.Tests.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GFLHApp.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public ProductsControllerTests()
        {
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(m => m.WebRootPath).Returns("wwwroot");
        }

        private void SetUser(Controller controller, string userId = "test-user", string role = "Standard")
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Index_AsStandardUser_ReturnsAllProducts()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();
            var producer = new Producers { ProducersId = 1, ProducerName = "Test Farm", UserId = "u1", ProducerEmail = "t@t.com", ProducerInformation = "info" };
            context.Producers.Add(producer);
            context.Products.AddRange(
                new Products { ProductsId = 1, ItemName = "Organic Apples", ItemPrice = 2.00m, Category = "Fruits", ProducersId = 1, Description = "desc" },
                new Products { ProductsId = 2, ItemName = "Whole Milk", ItemPrice = 1.80m, Category = "Dairy", ProducersId = 1, Description = "desc" }
            );
            await context.SaveChangesAsync();

            var controller = new ProductsController(context, _mockEnv.Object);
            SetUser(controller, "customer-1", "Standard");

            var result = await controller.Index(null);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Products>>(viewResult.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ValidId_ReturnsProductView()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();
            var producer = new Producers { ProducersId = 1, ProducerName = "Test Farm", UserId = "u1", ProducerEmail = "t@t.com", ProducerInformation = "info" };
            context.Producers.Add(producer);
            var product = new Products { ProductsId = 10, ItemName = "Artisan Cheese", ItemPrice = 4.20m, Category = "Dairy", ProducersId = 1, Description = "Delicious" };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var controller = new ProductsController(context, _mockEnv.Object);
            SetUser(controller);

            var result = await controller.Details(10);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Products>(viewResult.Model);
            Assert.Equal("Artisan Cheese", model.ItemName);
            Assert.Equal(4.20m, model.ItemPrice);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();
            var controller = new ProductsController(context, _mockEnv.Object);
            SetUser(controller);

            var result = await controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            using var context = TestDbHelper.GetInMemoryDbContext();
            var controller = new ProductsController(context, _mockEnv.Object);
            SetUser(controller);

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
