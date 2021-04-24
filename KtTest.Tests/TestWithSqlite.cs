using KtTest.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KtTest.Tests
{
    public abstract class TestWithSqlite : IDisposable
    {
        private const string InMemoryConnectionString = "DataSource=:memory:";
        private readonly SqliteConnection _connection;
        protected AppDbContext dbContext;

        protected TestWithSqlite()
        {
            _connection = new SqliteConnection(InMemoryConnectionString);
            _connection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connection)
                    .Options;
            dbContext = new AppDbContext(options);
            dbContext.Database.EnsureCreated();
        }

        public void InsertData<T>(T data) where T : class
        {
            dbContext.Set<T>().Add(data);
            dbContext.SaveChanges();
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
