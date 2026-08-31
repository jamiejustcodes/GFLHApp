using Microsoft.EntityFrameworkCore;
using GFLHApp.Data;
using System;

namespace GFLHApp.Tests.Helpers
{
    public static class TestDbHelper
    {
        public static ApplicationDbContext GetInMemoryDbContext(string dbName = "")
        {
            if (string.IsNullOrEmpty(dbName))
            {
                dbName = Guid.NewGuid().ToString();
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
