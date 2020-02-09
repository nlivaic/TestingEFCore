using System;
using System.Collections.Generic;
using System.Linq;
using CourseManager.API.DbContexts;
using CourseManager.API.Entities;
using CourseManager.API.Services;
using CourseManager.Tests.Logging;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace CourseManager.Tests
{
    public class AuthorsRepositorySQLiteTests
    {
        /// <summary>
        /// Won't output to .NET Test Launch window in VS Code.
        /// Will output to Test Output window in VS.!--
        /// Will output to CLI if started using `dotnet test`. 
        /// </summary>
        private readonly ITestOutputHelper _testOutput;

        public AuthorsRepositorySQLiteTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void GetAuthors_GetSecondPage_ReturnsSecondPage()
        {
            // Arrange
            var dbBuilder = new SQLiteDbBuilder(_testOutput);
            using (CourseContext context = dbBuilder.BuildContext())
            {
                context.Countries.Add(new Country()
                {
                    Id = "BE",
                    Description = "Belgium"
                });

                context.Countries.Add(new Country()
                {
                    Id = "US",
                    Description = "United States of America"
                });

                context.Authors.Add(new Author()
                { FirstName = "Kevin", LastName = "Dockx", CountryId = "BE" });
                context.Authors.Add(new Author()
                { FirstName = "Gill", LastName = "Cleeren", CountryId = "BE" });
                context.Authors.Add(new Author()
                { FirstName = "Julie", LastName = "Lerman", CountryId = "US" });
                context.Authors.Add(new Author()
                { FirstName = "Shawn", LastName = "Wildermuth", CountryId = "BE" });
                context.Authors.Add(new Author()
                { FirstName = "Deborah", LastName = "Kurata", CountryId = "US" });
                context.SaveChanges();
            }

            using (var context = dbBuilder.BuildContext())
            {
                // Act
                AuthorRepository target = new AuthorRepository(context);
                var result = target.GetAuthors(2, 3);

                // Assert
                Assert.Equal(2, result.Count());
                Assert.Same("Wildermuth", result.ToList()[0].LastName);
                Assert.Same("Kurata", result.ToList()[1].LastName);
            }
        }

        [Fact]
        public void GetAuthors_EmptyGuid_Throws_Argument_Exception()
        {
            // Arrange
            using (CourseContext context = new SQLiteDbBuilder(_testOutput).BuildContext())
            {
                var target = new AuthorRepository(context);

                // Assert
                Assert.Throws<ArgumentException>(
                    // Act
                    () => target.GetAuthor(Guid.Empty));
            }
        }

        [Fact]
        public void AddAuthor_AuthorWithoutCountry_DefaultsToBE()
        {
            List<string> logs = new List<string>();
            // Arrange
            var dbBuilder = new SQLiteDbBuilder(_testOutput, logs);
            var newAuthor = new Author
            {
                Id = new Guid("ae9118fd-4ffd-4895-a756-c371f656eeed"),
                FirstName = "New First Name",
                LastName = "New Last Name"
            };
            using (var context = dbBuilder.BuildContext())
            {
                context.Countries.Add(new Country()
                {
                    Id = "BE",
                    Description = "Belgium"
                });
                context.SaveChanges();
            }

            // Act
            using (var context = dbBuilder.BuildContext())
            {
                var target = new AuthorRepository(context);
                target.AddAuthor(newAuthor);
                target.SaveChanges();
            }

            // Assert
            using (var context = dbBuilder.BuildContext())
            {
                var target = new AuthorRepository(context);
                var addedAuthor = target.GetAuthor(
                    new Guid("ae9118fd-4ffd-4895-a756-c371f656eeed"));
                Assert.Equal("BE", context.Authors.Single().CountryId);
            }
        }
    }

    public class SQLiteDbBuilder
    {
        private readonly DbContextOptions<CourseContext> _options;

        /// <summary>
        /// Creates a new DbContext with an open database connection already set up.
        /// Make sure not to call `context.Database.OpenConnection()` from your code.
        /// </summary>
        public SQLiteDbBuilder(
            ITestOutputHelper testOutput,
            List<string> logs = null)   // This parameter is just for demo purposes, to show you can output logs.
        {
            var connectionString = new SqliteConnection("DataSource=:memory:");
            _options = new DbContextOptionsBuilder<CourseContext>()
                .UseLoggerFactory(new LoggerFactory(
                    new[] {
                        new TestLoggerProvider(
                            message => testOutput.WriteLine(message),
                            // message => logs?.Add(message),
                            LogLevel.Information
                        )
                    }
                ))
                .UseSqlite(connectionString)
                .Options;
            var context = new CourseContext(_options);
            // OpenConnection() and EnsureCreated() must be called only once per test method (on the same context).
            // By calling OpenConnection() we are creating a new database.
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
        }

        internal CourseContext BuildContext() =>
            new CourseContext(_options);
    }
}
