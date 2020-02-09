using System;
using System.Linq;
using CourseManager.API.DbContexts;
using CourseManager.API.Entities;
using CourseManager.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CourseManager.Tests
{
    public class AuthorsRepositoryInMemoryTests
    {
        [Fact]
        public void GetAuthors_GetSecondPage_ReturnsSecondPage()
        {
            // Arrange
            var dbBuilder = new InMemoryDbBuilder();
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
            using (CourseContext context = new InMemoryDbBuilder().BuildContext())
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
            // Arrange
            var dbBuilder = new InMemoryDbBuilder();
            var newAuthor = new Author
            {
                Id = new Guid("ae9118fd-4ffd-4895-a756-c371f656eeed"),
                LastName = "New Last Name"
            };
            using (var context = dbBuilder.BuildContext())
            {
                context.Countries.Add(new Country()
                {
                    Id = "BE",
                    Description = "Belgium"
                });
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

    public class InMemoryDbBuilder
    {
        private readonly DbContextOptions<CourseContext> _options;

        public InMemoryDbBuilder()
        {
            _options = new DbContextOptionsBuilder<CourseContext>()
                .UseInMemoryDatabase($"CourseLibraryInMemoryDatabase{Guid.NewGuid()}")
                .Options;
        }

        internal CourseContext BuildContext() =>
            new CourseContext(_options);
    }
}
