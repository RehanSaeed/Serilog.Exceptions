namespace Serilog.Exceptions.Test.Filters
{
    using System;
    using Serilog.Exceptions.Filters;
    using Xunit;

    public class CompositeExceptionPropertyFilterTest
    {
        [Fact]
        public void CreationOfCompositeFilter_ForNullFilters_Throws() =>
            Assert.Throws<ArgumentNullException>(
                () => new CompositeExceptionPropertyFilter(null!));

        [Fact]
        public void CreationOfCompositeFilter_ForEmptyFilters_Throws() =>
            Assert.Throws<ArgumentException>(
                () => new CompositeExceptionPropertyFilter(Array.Empty<IExceptionPropertyFilter>()));

        [Fact]
        public void CreationOfCompositeFilter_ForOneNullFilter_Throws()
        {
            // Act
            var ex = Assert.Throws<ArgumentException>(
                () => new CompositeExceptionPropertyFilter(new IExceptionPropertyFilter[] { null! }));

            // Assert
            Assert.Contains("index 0", ex.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void ShouldFilterTheProperty_WhenFirstFilterFilters_Filters()
        {
            // Arrange
            var filterA = new IgnorePropertyByNameExceptionFilter("A");
            var filterB = new IgnorePropertyByNameExceptionFilter("B");
            var composite = new CompositeExceptionPropertyFilter(filterA, filterB);

            // Act
            var shouldFilter = composite.ShouldPropertyBeFiltered(
                new Exception(),
                "B",
                1);

            // Assert
            Assert.True(shouldFilter);
        }

        [Fact]
        public void ShouldFilterTheProperty_WhenSecondFilterFilters_Filters()
        {
            // Arrange
            var filterA = new IgnorePropertyByNameExceptionFilter("A");
            var filterB = new IgnorePropertyByNameExceptionFilter("B");
            var composite = new CompositeExceptionPropertyFilter(filterA, filterB);

            // Act
            var shouldFilter = composite.ShouldPropertyBeFiltered(
                new Exception(),
                "A",
                1);

            // Assert
            Assert.True(shouldFilter);
        }

        [Fact]
        public void ShouldFilterTheProperty_WhenNoFilterFilters_NotFilters()
        {
            // Arrange
            var filterA = new IgnorePropertyByNameExceptionFilter("A");
            var filterB = new IgnorePropertyByNameExceptionFilter("B");
            var composite = new CompositeExceptionPropertyFilter(filterA, filterB);

            // Act
            var shouldFilter = composite.ShouldPropertyBeFiltered(
                new Exception(),
                "C",
                1);

            // Assert
            Assert.False(shouldFilter);
        }
    }
}
