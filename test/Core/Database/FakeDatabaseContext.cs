using System.Collections.Generic;
using System.Linq;
using main.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace test.Core.Database
{
    public class FakeDatabaseContext<T> where T: class
    {
        private readonly DbContextOptions<DatabaseContext> _contextOptions;
        
        protected FakeDatabaseContext()
        {
            _contextOptions = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase("database")
                .Options;
        }

        protected DatabaseContext Context() =>
            new DatabaseContext(_contextOptions);
        
        protected List<T> GetAll(DatabaseContext context) => 
            context.Set<T>().ToList();

        protected void Provision(DatabaseContext context, List<T> records)
        {
            context.Set<T>().RemoveRange(GetAll(context));
            context.SaveChanges();

            if (records != null)
            {
                context.Set<T>().AddRange(records);
                context.SaveChanges();
            }
        }
    }
}