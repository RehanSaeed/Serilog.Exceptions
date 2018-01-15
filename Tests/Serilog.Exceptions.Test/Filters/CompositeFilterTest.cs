namespace Serilog.Exceptions.Test.Filters
{
    using System;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Filters;
    using Xunit;

    public class CompositeFilterTest
    {
        [Fact]
        public void CreationOfCompositeFilter_ForNullFilters_Throws()
        {
            // Arrange
            Action act = () => new CompositeFilter(null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void CreationOfCompositeFilter_ForEmptyFilters_Throws()
        {
            // Arrange
            Action act = () => new CompositeFilter(new IExceptionPropertyFilter[] { });

            // Act & Assert
            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void CreationOfCompositeFilter_ForOneNullFilter_Throws()
        {
            // Arrange
            Action act = () => new CompositeFilter(new IExceptionPropertyFilter[] { null });

            // Act
            var ex = Assert.Throws<ArgumentException>(act);

            // Assert
            Assert.Contains("index 0", ex.Message);
        }

    }
}
