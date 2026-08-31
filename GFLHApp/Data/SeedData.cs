using GFLHApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GFLHApp.Data
{
    /// <summary>
    /// Handles initial data seeding for application roles, default user personas,
    /// and demonstration catalogue data for producers and products.
    /// </summary>
    public class SeedData
    {
        /// <summary>
        /// Seeds standard roles (Admin, Producer, Standard, Developer) and default user accounts.
        /// </summary>
        public static async Task SeedUsersAndRoles(
            IServiceProvider serviceProvider, 
            UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            // Seed authorization roles
            string[] roleNames = { "Admin", "Producer", "Standard", "Developer" };
            foreach (string roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed user personas with corresponding role assignments

            // 1. Admin Persona
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 2. Producer Personas (5 accounts for distinct farm suppliers)
            var producerEmails = new[]
            {
                "producer@example.com",
                "producer2@example.com",
                "producer3@example.com",
                "producer4@example.com",
                "producer5@example.com"
            };

            foreach (var email in producerEmails)
            {
                var producerUser = await userManager.FindByEmailAsync(email);
                if (producerUser == null)
                {
                    producerUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                    await userManager.CreateAsync(producerUser, "Password123!");
                }
                if (!await userManager.IsInRoleAsync(producerUser, "Producer"))
                {
                    await userManager.AddToRoleAsync(producerUser, "Producer");
                }
            }

            // 3. Developer Persona
            var devUser = await userManager.FindByEmailAsync("developer@example.com");
            if (devUser == null)
            {
                devUser = new IdentityUser { UserName = "developer@example.com", Email = "developer@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(devUser, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(devUser, "Developer"))
            {
                await userManager.AddToRoleAsync(devUser, "Developer");
            }

            // 4. Standard Customer Persona
            var normalUser = await userManager.FindByEmailAsync("user@example.com");
            if (normalUser == null)
            {
                normalUser = new IdentityUser { UserName = "user@example.com", Email = "user@example.com", EmailConfirmed = true };
                await userManager.CreateAsync(normalUser, "Password123!");
            }
            if (!await userManager.IsInRoleAsync(normalUser, "Standard"))
            {
                await userManager.AddToRoleAsync(normalUser, "Standard");
            }
        }

        /// <summary>
        /// Seeds sample local producer profiles linked to the seeded producer user accounts.
        /// </summary>
        public static async Task SeedProducers(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var producerUser1 = await userManager.FindByEmailAsync("producer@example.com");
            var producerUser2 = await userManager.FindByEmailAsync("producer2@example.com");
            var producerUser3 = await userManager.FindByEmailAsync("producer3@example.com");
            var producerUser4 = await userManager.FindByEmailAsync("producer4@example.com");
            var producerUser5 = await userManager.FindByEmailAsync("producer5@example.com");

            if (producerUser1 == null || producerUser2 == null || producerUser3 == null || producerUser4 == null || producerUser5 == null)
            {
                throw new Exception("Producer users not found.");
            }

            // Avoid duplicate seeding
            if (context.Producers.Any())
                return;

            var producers = new List<Producers>
            {
                new Producers
                {
                    ProducerName = "Green Valley Produce",
                    ProducerEmail = "greenvalley@farmmail.co.uk",
                    ProducerInformation = "Green Valley Produce is a family-owned farm located in the heart of the countryside. We specialize in growing organic fruits and vegetables using sustainable farming practices. Our farm has been in operation for over 50 years, and we take pride in providing fresh, high-quality produce to our customers.",
                    UserId = producerUser1.Id,
                    VATNumber = "GB863574975",
                    IsVATRegistered = true,
                    ImagePath = "/images/producers/greenValleyProduce.jpg"
                },
                new Producers
                {
                    ProducerName = "Sunny Brook Farm",
                    ProducerEmail = "sunnybrook@ruralmail.co.uk",
                    ProducerInformation = "Sunny Brook Farm is a small-scale farm that focuses on raising free-range poultry and producing farm-fresh eggs. We are committed to animal welfare and sustainable farming methods. Our chickens roam freely in spacious pastures, and we provide them with a nutritious diet to ensure the highest quality eggs for our customers.",
                    UserId = producerUser2.Id,
                    VATNumber = "GB763906214",
                    IsVATRegistered = true,
                    ImagePath = "/images/producers/sunnybrookFarm.jpg"
                },
                new Producers
                {
                    ProducerName = "Oakridge Harvest Hub",
                    ProducerEmail = "oakridge@harvesthub.co.uk",
                    ProducerInformation = "Oakridge Harvest Hub is a cooperative of local farmers who come together to share resources and market their produce. We grow a wide variety of fruits, vegetables, and herbs using organic and regenerative farming techniques. Our mission is to support local agriculture and provide fresh, seasonal produce to our community.",
                    UserId = producerUser3.Id,
                    VATNumber = "GB846385085",
                    IsVATRegistered = true,
                    ImagePath = "/images/producers/oakridgeHarvestHub.jpg"
                },
                new Producers
                {
                    ProducerName = "Meadow Fresh Organic Farms",
                    ProducerEmail = "meadowfresh@agromail.co.uk",
                    ProducerInformation = "Meadow Fresh Organic Farms is dedicated to producing high-quality organic dairy products",
                    UserId = producerUser4.Id,
                    IsVATRegistered = false,
                    ImagePath = "/images/producers/meadowFreshOrganicFarm.jpg"
                },
                new Producers
                {
                    ProducerName = "Willowcreek Farm",
                    ProducerEmail = "willowcreek@farmconnect.co.uk",
                    ProducerInformation = "Willowcreek Farm is a family-run farm that specializes in growing heirloom vegetables and artisanal herbs. We are passionate about preserving traditional farming methods and cultivating unique, flavorful produce. Our farm is committed to sustainability and biodiversity, and we strive to create a thriving ecosystem on our land.",
                    UserId = producerUser5.Id,
                    IsVATRegistered = false,
                    ImagePath = "/images/producers/willowcreekFarm.jpg"
                }
            };

            context.Producers.AddRange(producers);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Seeds sample products across producers, with category, pricing, stock levels, and allergen information.
        /// </summary>
        public static async Task SeedProducts(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var GreenValleyProducer = await context.Producers.FirstOrDefaultAsync(x => x.ProducerName == "Green Valley Produce");
            var SunnyBrookFarm = await context.Producers.FirstOrDefaultAsync(x => x.ProducerName == "Sunny Brook Farm");
            var OakridgeHarvestHub = await context.Producers.FirstOrDefaultAsync(x => x.ProducerName == "Oakridge Harvest Hub");
            var MeadowFreshOrganicFarms = await context.Producers.FirstOrDefaultAsync(x => x.ProducerName == "Meadow Fresh Organic Farms");
            var WillowcreekFarm = await context.Producers.FirstOrDefaultAsync(x => x.ProducerName == "Willowcreek Farm");

            if (GreenValleyProducer == null || SunnyBrookFarm == null || OakridgeHarvestHub == null || MeadowFreshOrganicFarms == null || WillowcreekFarm == null)
            {
                throw new Exception("Producers not found.");
            }


            if (!context.Products.Any())
            {
                var products = new List<Products>
                {
                    // Seed products for Green Valley Produce
                    new Products
                    {
                        ItemName = "Apple",
                        QuantityInStock = 100,
                        ItemPrice = 1.00m,
                        Description = "Our crisp and juicy apples are handpicked at the peak of ripeness to ensure maximum flavor and freshness. Grown with care on our farm, these apples are perfect for snacking, baking, or adding to salads. Each bite offers a deliciously sweet and slightly tart flavor that will delight your taste buds.",
                        Category = "Fruit",
                        ProducersId = GreenValleyProducer.ProducersId, 
                        ImagePath = "/images/products/apple.jpg",
                        Available = true
                    },

                    new Products
                    {
                        ItemName = "Broccoli",
                        QuantityInStock = 80,
                        ItemPrice = 1.40m,
                        Description = "Our fresh and crisp broccoli is perfect for roasting, steaming, or adding to stir-fries. Grown with care on our farm, this broccoli is packed with nutrients and has a deliciously mild flavor that pairs well with a variety of dishes.",
                        Category = "Vegetable",
                        ProducersId = GreenValleyProducer.ProducersId,
                        ImagePath = "/images/products/broccoli.jpg",
                        Available = true
                    },

                    new Products
                    {
                        ItemName = "Carrot",
                        QuantityInStock = 140,
                        ItemPrice = 0.70m,
                        Description = "Our sweet and crunchy carrots are perfect for snacking or adding to salads. Grown with care on our farm, these carrots are harvested at the peak of freshness to ensure maximum flavor and quality.",
                        Category = "Vegetable",
                        ProducersId = GreenValleyProducer.ProducersId,
                        ImagePath = "/images/products/carrot.jpg",
                        Available = false

                    },

                    new Products
                    {
                        ItemName = "Cauliflower",
                        QuantityInStock = 60,
                        ItemPrice = 1.80m,
                        Description = "Our fresh and tender cauliflower is perfect for roasting, steaming, or adding to soups and stews. Grown with care on our farm, this cauliflower is harvested at the peak of freshness to ensure maximum flavor and quality.",
                        Category = "Vegetable",
                        ProducersId = GreenValleyProducer.ProducersId,
                        ImagePath = "/images/products/cauliflower.jpg",
                        Available = true
                    },

                    new Products
                    {
                        ItemName = "Potato",
                        QuantityInStock = 120,
                        ItemPrice = 0.45m,
                        Description = "Our farm-fresh potatoes are harvested at the peak of freshness to ensure maximum flavor and quality. Grown with care on our farm, these potatoes are perfect for roasting, mashing, or frying. They have a deliciously earthy flavor and a fluffy texture that will elevate any dish.",
                        Category = "Vegetable",
                        ProducersId = GreenValleyProducer.ProducersId,
                        ImagePath = "/images/products/potatoes.jpg",
                        Available = true

                    },
                    // Seed products for Sunny Brook Farm
                    new Products
                    {
                        ItemName = "Free-Range Eggs",
                        QuantityInStock = 120,
                        ItemPrice = 0.50m,
                        Description = "Our farm-fresh free-range eggs are produced by happy, healthy chickens that roam freely in spacious pastures. These eggs are rich in flavor and packed with nutrients, making them a delicious and wholesome choice for your meals. Whether you're baking, frying, or scrambling, our free-range eggs will add a fresh and wholesome touch to your dishes.",
                        Category = "Dairy",
                        ProducersId = SunnyBrookFarm.ProducersId,
                        ImagePath = "/images/products/egg.jpg",
                        Available = false,
                        Allergens = "Eggs"

                     },

                    new Products
                    {
                        ItemName = "Chicken breast",
                        QuantityInStock = 65,
                        ItemPrice = 4.80m,
                        Description = "Our farm-fresh chicken breast is sourced from free-range chickens that are raised on a diet of natural feed and have access to the outdoors. The result is tender, juicy chicken with a deliciously fresh taste. Our commitment to sustainable farming practices means that you can enjoy our chicken breast with confidence, knowing that it is not only good for you but also good for the environment.",
                        Category = "Meat",
                        ProducersId = SunnyBrookFarm.ProducersId,
                        ImagePath = "/images/products/chickenbreast.jpg",
                        Available = false
                     },

                    new Products
                    {
                        ItemName = "Cheese",
                        QuantityInStock = 90,
                        ItemPrice = 3.80m,
                        Description = "Our farm-fresh cheese is crafted with care using milk from our grass-fed cows. It has a rich, creamy texture and a deliciously tangy flavor that pairs perfectly with crackers, bread, or fresh fruit. Our cheese is made using traditional methods and is free from artificial additives, ensuring a wholesome and natural taste in every bite.",
                        Category = "Dairy",
                        ProducersId = SunnyBrookFarm.ProducersId,
                        ImagePath = "/images/products/cheese.jpg",
                        Available = true,
                        Allergens = "Dairy"
                     },

                    new Products
                    {
                        ItemName = "Yogurt",
                        QuantityInStock = 75,
                        ItemPrice = 1.10m,
                        Description = "Our farm-fresh yogurt is made from the freshest milk produced by our happy, grass-fed cows. It is creamy, rich, and packed with probiotics that promote a healthy gut. Our yogurt is available in a variety of delicious flavors, all made with natural ingredients and no artificial additives. Whether you enjoy it on its own, with fruit, or as a base for smoothies and desserts, our farm-fresh yogurt is a wholesome and tasty choice for your daily diet.",
                        Category = "Dairy",
                        ProducersId = SunnyBrookFarm.ProducersId,
                        ImagePath = "/images/products/yogurt.jpg",
                        Available = true,
                        Allergens = "Dairy"
                     },

                    new Products
                    {
                        ItemName = "Milk",
                        QuantityInStock = 90,
                        ItemPrice = 1.20m,
                        Description = "Our farm-fresh milk is sourced from our happy, grass-fed cows that graze on lush pastures. It is creamy, rich, and packed with nutrients, making it a delicious and wholesome choice for your daily diet. Our commitment to sustainable farming practices ensures that our milk is not only good for you but also good for the environment.",
                        Category = "Dairy",
                        ProducersId = SunnyBrookFarm.ProducersId,
                        ImagePath = "/images/products/milk.jpg",
                        Available = true,
                        Allergens = "Dairy"

                     },

                    // Seed products for Oakridge Harvest Hub

                    new Products
                    {
                        ItemName = "Strawberry",
                        QuantityInStock = 70,
                        ItemPrice = 2.00m,
                        Description = "Our sweet and juicy strawberries are handpicked at the peak of ripeness to ensure maximum flavor and freshness. Grown with care on our farm, these strawberries are perfect for snacking, adding to desserts, or blending into smoothies. Each bite bursts with a deliciously sweet and slightly tart flavor that will delight your taste buds.",
                        Category = "Fruit",
                        ProducersId = OakridgeHarvestHub.ProducersId,
                        ImagePath = "/images/products/strawberry.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Apple",
                        QuantityInStock = 100,
                        ItemPrice = 0.85m,
                        Description = "Our crisp and juicy apples are handpicked at the peak of ripeness to ensure maximum flavor and freshness. Grown with care on our farm, these apples are perfect for snacking, baking, or adding to salads. Each bite offers a deliciously sweet and slightly tart flavor that will delight your taste buds.",
                        Category = "Fruit",
                        ProducersId = OakridgeHarvestHub.ProducersId,
                        ImagePath = "/images/products/apple.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Pear",
                        QuantityInStock = 80,
                        ItemPrice = 0.80m,
                        Description = "Our juicy and sweet pears are handpicked at the peak of ripeness to ensure maximum flavor and freshness. Grown with care on our farm, these pears are perfect for snacking or adding to salads. Each bite offers a deliciously sweet and slightly tart flavor that will delight your taste buds.",
                        Category = "Fruit",
                        ProducersId = OakridgeHarvestHub.ProducersId,
                        ImagePath = "/images/products/pear.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Raspberry",
                        QuantityInStock = 55,
                        ItemPrice = 2.30m,
                        Description = "Our sweet and juicy raspberries are handpicked at the peak of ripeness to ensure maximum flavor and freshness. Grown with care on our farm, these raspberries are perfect for snacking, adding to desserts, or blending into smoothies. Each bite bursts with a deliciously sweet and slightly tart flavor that will delight your taste buds.",
                        Category = "Fruit",
                        ProducersId = OakridgeHarvestHub.ProducersId,
                        ImagePath = "/images/products/raspberry.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Broccoli",
                        QuantityInStock = 85,
                        ItemPrice = 1.10m,
                        Description = "Our fresh and crisp broccoli is perfect for roasting, steaming, or adding to stir-fries. Grown with care on our farm, this broccoli is packed with nutrients and has a deliciously mild flavor that pairs well with a variety of dishes.",
                        Category = "Vegetable",
                        ProducersId = OakridgeHarvestHub.ProducersId,
                        ImagePath = "/images/products/broccoli.jpg",
                        Available = false
                     },

                    // Seed products for Meadow Fresh Organic Farms

                    new Products
                    {
                        ItemName = "Organic Milk",
                        QuantityInStock = 95,
                        ItemPrice = 1.30m,
                        Description = "Our organic milk is sourced from cows that are raised on lush, green pastures and fed a natural diet free from synthetic hormones and antibiotics. The result is a creamy, rich milk with a deliciously fresh taste that you can feel good about. Our commitment to sustainable farming practices ensures that our milk is not only good for you but also good for the environment.",
                        Category = "Dairy",
                        ProducersId = MeadowFreshOrganicFarms.ProducersId,
                        ImagePath = "/images/products/milk.jpg",
                        Available = true,
                        Allergens = "Dairy"
                     },

                    new Products
                    {
                        ItemName = "Cheese",
                        QuantityInStock = 50,
                        ItemPrice = 4.20m,
                        Description = "Our organic cheese is crafted with care using milk from our grass-fed cows. It has a rich, creamy texture and a deliciously tangy flavor that pairs perfectly with crackers, bread, or fresh fruit. Our cheese is made using traditional methods and is free from artificial additives, ensuring a wholesome and natural taste in every bite.",
                        Category = "Dairy",
                        ProducersId = MeadowFreshOrganicFarms.ProducersId,
                        ImagePath = "/images/products/cheese.jpg",
                        Available = true,
                        Allergens = "Dairy"
                     },

                    new Products
                    {
                        ItemName = "Yogurt",
                        QuantityInStock = 70,
                        ItemPrice = 1.30m,
                        Description = "Our organic yogurt is made from the freshest milk produced by our happy, grass-fed cows. It is creamy, rich, and packed with probiotics that promote a healthy gut. Our yogurt is available in a variety of delicious flavors, all made with natural ingredients and no artificial additives. Whether you enjoy it on its own, with fruit, or as a base for smoothies and desserts, our organic yogurt is a wholesome and tasty choice for your daily diet.",
                        Category = "Dairy",
                        ProducersId = MeadowFreshOrganicFarms.ProducersId,
                        ImagePath = "/images/products/yogurt.jpg",
                        Available = true,
                        Allergens = "Dairy"
                     },

                    new Products
                    {
                        ItemName = "Egg",
                        QuantityInStock = 100,
                        ItemPrice = 0.40m,
                        Description = "Our organic eggs are produced by free-range chickens that are raised on a diet of organic feed and have access to the outdoors. These eggs are rich in flavor and packed with nutrients, making them a healthy and delicious choice for your meals. Whether you're baking, frying, or scrambling, our organic eggs will add a fresh and wholesome touch to your dishes.",
                        Category = "Dairy",
                        ProducersId = MeadowFreshOrganicFarms.ProducersId,
                        ImagePath = "/images/products/egg.jpg",
                        Available = true,
                        Allergens = "Eggs"
                     },

                    new Products
                    {
                        ItemName = "Mince Meat",
                        QuantityInStock = 55,
                        ItemPrice = 5.50m,
                        Description = "Our organic mince meat is made from high-quality, grass-fed beef that is raised without the use of antibiotics or hormones. It is carefully ground to ensure a consistent texture and is perfect for making delicious meals like burgers, meatballs, and spaghetti bolognese. Our commitment to sustainable farming practices means that you can enjoy our mince meat with confidence, knowing that it is not only good for you but also good for the environment.",
                        Category = "Meat",
                        ProducersId = MeadowFreshOrganicFarms.ProducersId,
                        ImagePath = "/images/products/mincemeat.jpg",
                        Available = true
                     },

                    // Seed products for Willowcreek Farm

                    new Products
                    {
                        ItemName = "Carrot",
                        QuantityInStock = 130,
                        ItemPrice = 0.75m,
                        Description = "A sweet and crunchy carrot, perfect for snacking or adding to salads. Grown with care on our farm, these carrots are harvested at the peak of freshness to ensure maximum flavor and quality.",
                        Category = "Vegetable",
                        ProducersId = WillowcreekFarm.ProducersId,
                        ImagePath = "/images/products/carrot.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Cauliflower",
                        QuantityInStock = 65,
                        ItemPrice = 1.70m,
                        Description = "A fresh and tender cauliflower, perfect for roasting, steaming, or adding to soups and stews. Grown with care on our farm, this cauliflower is harvested at the peak of freshness to ensure maximum flavor and quality.",
                        Category =  "Vegetable",
                        ProducersId = WillowcreekFarm.ProducersId,
                        ImagePath = "/images/products/cauliflower.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Potato",
                        QuantityInStock = 110,
                        ItemPrice = 0.40m,
                        Description = "A versatile and hearty potato, perfect for roasting, mashing, or frying. Grown with care on our farm, these potatoes are harvested at the peak of freshness to ensure maximum flavor and quality.",
                        Category = "Vegetable",
                        ProducersId = WillowcreekFarm.ProducersId,
                        ImagePath = "/images/products/potatoes.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Broccoli",
                        QuantityInStock = 75,
                        ItemPrice = 1.30m,
                        Description = "A fresh and crisp broccoli, perfect for roasting, steaming, or adding to stir-fries. Grown with care on our farm, this broccoli is packed with nutrients and has a deliciously mild flavor that pairs well with a variety of dishes.",
                        Category = "Vegetable",
                        ProducersId = WillowcreekFarm.ProducersId,
                        ImagePath = "/images/products/broccoli.jpg",
                        Available = true

                     },

                    new Products
                    {
                        ItemName = "Pear",
                        QuantityInStock = 70,
                        ItemPrice = 0.90m,
                        Description = "A juicy and sweet pear, perfect for snacking or adding to salads. Grown with care on our farm, these pears are harvested at the peak of ripeness to ensure maximum flavor and freshness.",
                        Category = "Fruit",
                        ProducersId = WillowcreekFarm.ProducersId,
                        ImagePath = "/images/products/pear.jpg",
                        Available = false
                     },


                };
                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }
}
