namespace Serilog.Exceptions.Test.Destructurers;

using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Formatting.Json;
using Xunit;
using static LogJsonOutputUtils;

public class DbUpdateExceptionDestructurerTest
{
    [Fact]
    public void WithoutDbUpdateExceptionDestructurerShouldLogDbValues()
    {
        var jsonWriter = new StringWriter();
        ILogger logger = new LoggerConfiguration()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers())
            .WriteTo.Sink(new TestTextWriterSink(jsonWriter, new JsonFormatter()))
            .CreateLogger();
        using var ctx = new TestContext();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();

        var entry = ctx.Add(new User
        {
            UserId = Guid.NewGuid().ToString(),
        });

        logger.Error(new TestDbUpdateException("DbUpdate Error", entry), "Error");

        var writtenJson = jsonWriter.ToString();
        Assert.True(writtenJson.Contains(TestContext.UserIdIDoNotWantToSee) ||
            writtenJson.Contains("\"Users\":\"threw System.TypeInitializationException"));
    }

    [Fact]
    public void WithDbUpdateExceptionDestructurerShouldNotLogDbValues()
    {
        var jsonWriter = new StringWriter();
        ILogger logger = new LoggerConfiguration()
            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers().WithDestructurers([new TestDbUpdateExceptionDestructurer()]))
            .WriteTo.Sink(new TestTextWriterSink(jsonWriter, new JsonFormatter()))
            .CreateLogger();
        using var ctx = new TestContext();
        ctx.Database.EnsureDeleted();
        ctx.Database.EnsureCreated();

        var entry = ctx.Add(new User
        {
            UserId = Guid.NewGuid().ToString(),
        });

        logger.Error(new TestDbUpdateException("DbUpdate Error", entry), "Error");

        var writtenJson = jsonWriter.ToString();
        Assert.DoesNotContain(TestContext.UserIdIDoNotWantToSee, writtenJson, StringComparison.Ordinal);
    }

    internal class User
    {
        public string? UserId { get; set; }
    }

    internal class TestContext : DbContext
    {
        public const string UserIdIDoNotWantToSee = "I Don't Want To See You";

        public DbSet<User>? Users { get; set; }

        public string CustomData { get; set; } = UserIdIDoNotWantToSee;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(databaseName: "TestDebUpdateException");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            List<User> users =
            [
                new() { UserId = "FirstUser" },
                new() { UserId = UserIdIDoNotWantToSee }
            ];

            modelBuilder.Entity<User>().HasData(users);
        }
    }

    internal class TestDbUpdateException : DbUpdateException
    {
        private readonly IReadOnlyList<EntityEntry> entityEntries;

        public TestDbUpdateException(string message, EntityEntry entityEntry)
            : base(message, (Exception)null!) => this.entityEntries = new List<EntityEntry> { entityEntry }.AsReadOnly();

        public override IReadOnlyList<EntityEntry> Entries => this.entityEntries;
    }

    internal class TestDbUpdateExceptionDestructurer : DbUpdateExceptionDestructurer
    {
        public override Type[] TargetTypes => [typeof(TestDbUpdateException)];
    }
}
