namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Serilog.Exceptions.Core;
    using Exceptions.Destructurers;
    using Xunit;

    public class ReflectionBasedDestructurerTest
    {
        private readonly ReflectionBasedDestructurer destructurer;

        public ReflectionBasedDestructurerTest()
        {
            this.destructurer = new ReflectionBasedDestructurer();
        }

        [Fact]
        public void Destructure_()
        {
            Exception exception;
            try
            {
                throw new TestException();
            }
            catch (Exception e)
            {
                exception = e;
            }

            var propertiesBag = new ExceptionPropertiesBag(typeof(Exception));

            this.destructurer.Destructure(exception, propertiesBag, null);

            var properties = propertiesBag.GetResultDictionary();
            Assert.Equal("PublicValue", properties[nameof(TestException.PublicProperty)]);
            Assert.Equal("threw System.Exception: Exception of type 'System.Exception' was thrown.", properties[nameof(TestException.ExceptionProperty)]);
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "InternalProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "ProtectedProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "PrivateProperty"));
            Assert.Equal("MessageValue", properties[nameof(TestException.Message)]);
            var data = Assert.IsType<Dictionary<string, object>>(properties[nameof(TestException.Data)]);
            Assert.Empty(data);
            Assert.Null(properties[nameof(TestException.InnerException)]);
#if NET461
            Assert.StartsWith("Void Destructure_(", properties[nameof(TestException.TargetSite)].ToString());
#endif
            Assert.NotEmpty(properties[nameof(TestException.StackTrace)].ToString());
            Assert.Null(properties[nameof(TestException.HelpLink)]);
            Assert.Equal("Serilog.Exceptions.Test", properties[nameof(TestException.Source)]);
            Assert.Equal(-2146233088, properties[nameof(TestException.HResult)]);
            Assert.Contains(typeof(TestException).FullName, properties["Type"].ToString());
        }

        public class TestException : Exception
        {
            public TestException()
                : base("MessageValue")
            {
                StaticProperty = "StaticValue";
                this.PublicProperty = "PublicValue";
                this.InternalProperty = "InternalValue";
                this.ProtectedProperty = "ProtectedValue";
                this.PrivateProperty = "PrivateValue";
            }

            public static string StaticProperty { get; set; }

            public string PublicProperty { get; set; }

            public string ExceptionProperty
            {
                get { throw new Exception(); }
            }

            internal string InternalProperty { get; set; }

            protected string ProtectedProperty { get; set; }

            private string PrivateProperty { get; set; }

            public string this[int i]
            {
                get { return "IndexerValue"; }
            }
        }
    }
}
