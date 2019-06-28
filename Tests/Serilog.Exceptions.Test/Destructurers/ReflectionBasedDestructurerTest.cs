namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Serilog.Exceptions.Core;
    using Serilog.Exceptions.Destructurers;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ReflectionBasedDestructurerTest
    {
        [Fact]
        public void DestructureComplexException_EachTypeOfPropertyIsDestructuredAsExpected()
        {
            // Arrange
            var exception = ThrowAndCatchException(() => throw new TestException());
            var propertiesBag = new ExceptionPropertiesBag(exception);

            // Act
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            // Assert
            var properties = propertiesBag.GetResultDictionary();
            Assert.Equal("PublicValue", properties[nameof(TestException.PublicProperty)]);
            Assert.Equal("threw System.Exception: Exception of type 'System.Exception' was thrown.", properties[nameof(TestException.ExceptionProperty)]);
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "InternalProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "ProtectedProperty"));
            Assert.DoesNotContain(properties, x => string.Equals(x.Key, "PrivateProperty"));
            Assert.Equal("MessageValue", properties[nameof(TestException.Message)]);
#if NET461 || NET472
            Assert.StartsWith("Void DestructureComplexException_EachTypeOfPropertyIsDestructuredAsExpected(", properties[nameof(TestException.TargetSite)].ToString());
#endif
            Assert.NotEmpty(properties[nameof(TestException.StackTrace)].ToString());
            Assert.Equal("Serilog.Exceptions.Test", properties[nameof(TestException.Source)]);
            Assert.Equal(-2146233088, properties[nameof(TestException.HResult)]);
            Assert.Contains(typeof(TestException).FullName, properties["Type"].ToString());
        }

        [Fact]
        public void CanDestructureUriProperty()
        {
            const string uriValue = "http://localhost/property";
            var exception = new UriException("test", new Uri(uriValue));

            var propertiesBag = new ExceptionPropertiesBag(exception);
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            var properties = propertiesBag.GetResultDictionary();
            var uriPropertyValue = properties[nameof(UriException.Uri)];
            Assert.IsType<string>(uriPropertyValue);
            Assert.Equal(uriValue, uriPropertyValue);
        }

        [Fact]
        public void CanDestructureUriDataItem()
        {
            const string uriValue = "http://localhost/data-item";
            var exception = new Exception("test")
            {
                Data =
                {
                    { "UriDataItem", new Uri(uriValue) }
                }
            };

            var propertiesBag = new ExceptionPropertiesBag(exception);
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            var properties = propertiesBag.GetResultDictionary();
            var data = (IDictionary)properties[nameof(Exception.Data)];
            var uriDataValue = data["UriDataItem"];
            Assert.IsType<string>(uriDataValue);
            Assert.Equal(uriValue, uriDataValue);
        }

        [Fact]
        public void CanDestructureTask()
        {
            Task task = new TaskFactory<int>().StartNew(() => 12, TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness);
            var exception = new TaskCanceledException(task);

            var propertiesBag = new ExceptionPropertiesBag(exception);
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            var properties = propertiesBag.GetResultDictionary();
            var destructuredTaskObject = (IDictionary)properties[nameof(TaskCanceledException.Task)];
            var destructuredTaskProperties = Assert.IsAssignableFrom<IDictionary<string, object>>(destructuredTaskObject);
            destructuredTaskProperties.Should().ContainKey(nameof(Task.Id));
            destructuredTaskProperties.Should().ContainKey(nameof(Task.Status))
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Be(nameof(TaskStatus.RanToCompletion));
            destructuredTaskProperties.Should().ContainKey(nameof(Task.CreationOptions))
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Contain(nameof(TaskCreationOptions.LongRunning))
                .And.Contain(nameof(TaskCreationOptions.PreferFairness));
        }

        [Fact]
        public void CanDestructureFaultedTask()
        {
            var taskException = new Exception("INNER EXCEPTION MESSAGE");
            var task = Task.FromException(taskException);
            var exception = new TaskException("TASK EXCEPTION MESSAGE", task);

            var propertiesBag = new ExceptionPropertiesBag(exception);
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, InnerDestructurer(CreateReflectionBasedDestructurer()));

            var properties = propertiesBag.GetResultDictionary();
            var destructuredTaskObject = (IDictionary)properties[nameof(TaskCanceledException.Task)];
            var destructuredTaskProperties = Assert.IsAssignableFrom<IDictionary<string, object>>(destructuredTaskObject);
            destructuredTaskProperties.Should().ContainKey(nameof(Task.Id));
            destructuredTaskProperties.Should().ContainKey(nameof(Task.Status))
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Be(nameof(TaskStatus.Faulted));
            destructuredTaskProperties.Should().ContainKey(nameof(Task.CreationOptions))
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Be(nameof(TaskCreationOptions.None));
            var taskFirstLevelExceptionDictionary = destructuredTaskProperties.Should().ContainKey(nameof(Task.Exception))
                .WhichValue.Should().BeAssignableTo<IDictionary<string, object>>()
                .Which;
            taskFirstLevelExceptionDictionary.Should().ContainKey("Message")
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Contain("One or more errors occurred.", "task's first level exception is aggregate exception");
            taskFirstLevelExceptionDictionary.Should().ContainKey("InnerExceptions")
                .WhichValue.Should().BeAssignableTo<IReadOnlyCollection<object>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeAssignableTo<IDictionary<string, object>>()
                .Which.Should().ContainKey("Message")
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Be("INNER EXCEPTION MESSAGE");
        }

        [Fact]
        public void CanDestructureStructDataItem()
        {
            // Arrange
            var exception = new Exception("test");
            exception.Data["data"] = new TestStruct()
            {
                ValueType = 10,
                ReferenceType = "ABC"
            };
            var propertiesBag = new ExceptionPropertiesBag(exception);

            // Act
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            // Assert
            var properties = propertiesBag.GetResultDictionary();
            var data = (IDictionary)properties[nameof(Exception.Data)];
            var testStructDataValue = data["data"];
            Assert.IsAssignableFrom<TestStruct>(testStructDataValue);
        }

        [Fact]
        public void CanDestructureClassDataItem()
        {
            // Arrange
            var exception = new Exception("test");
            exception.Data["data"] = new TestClass()
            {
                ValueType = 10,
                ReferenceType = "ABC"
            };
            var propertiesBag = new ExceptionPropertiesBag(exception);

            // Act
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            // Assert
            var properties = propertiesBag.GetResultDictionary();
            var data = (IDictionary)properties[nameof(Exception.Data)];
            var testStructDataValue = data["data"];
            var destructuredStructDictionary = Assert.IsAssignableFrom<IDictionary<string, object>>(testStructDataValue);
            Assert.Equal(10, destructuredStructDictionary[nameof(TestClass.ValueType)]);
            Assert.Equal("ABC", destructuredStructDictionary[nameof(TestClass.ReferenceType)]);
        }

        [Fact]
        public void DestructuringDepthIsLimitedByConfiguredDepth()
        {
            // Arrange
            var exception = new RecursiveException()
            {
                Node = new RecursiveNode()
                {
                    Name = "PARENT",
                    Child = new RecursiveNode()
                    {
                        Name = "CHILD 1",
                        Child = new RecursiveNode()
                        {
                            Name = "CHILD 2"
                        }
                    }
                }
            };
            var destructurer = new ReflectionBasedDestructurer(1);

            // Act
            var propertiesBag = new ExceptionPropertiesBag(exception);
            destructurer.Destructure(exception, propertiesBag, EmptyDestructurer());

            // Assert
            // Parent is depth 1
            // First child is depth 2
            var properties = propertiesBag.GetResultDictionary();
            var parent = (IDictionary<string, object>)properties[nameof(RecursiveException.Node)];
            Assert.Equal("PARENT", parent[nameof(RecursiveNode.Name)]);
            Assert.IsType<RecursiveNode>(parent[nameof(RecursiveNode.Child)]);
        }

        [Fact]
        public void ExceptionWithTypeProperty_StillContainsType_JustWithDollarAsPrefixInLabel()
        {
            var exceptionWithTypeProperty = new ExceptionWithTypeProperty() { Type = 13 };
            Test_LoggedExceptionContainsProperty(exceptionWithTypeProperty, "$Type", $"Serilog.Exceptions.Test.Destructurers.{nameof(ReflectionBasedDestructurerTest)}+ExceptionWithTypeProperty");
        }

        [Fact]
        public void WhenObjectContainsCyclicReferences_ThenNoStackoverflowExceptionIsThrown()
        {
            // Arrange
            var exception = new CyclicException
            {
                MyObject = new MyObject()
            };
            exception.MyObject.Foo = "bar";
            exception.MyObject.Reference = exception.MyObject;
            exception.MyObject.Reference2 = exception.MyObject;

            // Act
            var result = new ExceptionPropertiesBag(new Exception());
            var destructurer = new ReflectionBasedDestructurer(10);
            destructurer.Destructure(exception, result, EmptyDestructurer());

            // Assert
            var myObject = (Dictionary<string, object>)result.GetResultDictionary()["MyObject"];

            Assert.Equal("bar", myObject["Foo"]);
            Assert.Equal(myObject["$id"], ( (Dictionary<string, object>)myObject["Reference"] )["$ref"]);
            Assert.Equal(myObject["$id"], ( (Dictionary<string, object>)myObject["Reference2"] )["$ref"]);
            Assert.Equal("1", myObject["$id"]);
        }

        [Fact]
        public void WhenObjectContainsCyclicReferencesInList_ThenRecursiveDestructureIsImmediatelyStopped()
        {
            // Arrange
            var cyclic = new MyObjectEnumerable
            {
                Foo = "Cyclic"
            };
            cyclic.Reference = cyclic;
            var exception = new CyclicException2
            {
                MyObjectEnumerable = new MyObjectEnumerable()
            };
            exception.MyObjectEnumerable.Foo = "bar";
            exception.MyObjectEnumerable.Reference = cyclic;

            // Act
            var result = new ExceptionPropertiesBag(new Exception());
            var destructurer = new ReflectionBasedDestructurer(10);
            destructurer.Destructure(exception, result, EmptyDestructurer());

            // Assert
            var myObject = (List<object>)result.GetResultDictionary()["MyObjectEnumerable"];

            // exception.MyObjectEnumerable[0] is still list
            var firstLevelList = Assert.IsType<List<object>>(myObject[0]);

            // exception.MyObjectEnumerable[0][0] we notice that we would again destructure "cyclic"
            var secondLevelList = Assert.IsType<Dictionary<string, object>>(firstLevelList[0]);
            Assert.Equal("Cyclic reference", secondLevelList["$ref"]);
        }

        [Fact]
        public void WhenObjectContainsCyclicReferencesInDict_ThenRecursiveDestructureIsImmediatelyStopped()
        {
            // Arrange
            var cyclic = new MyObjectDict
            {
                Foo = "Cyclic",
                Reference = new Dictionary<string, object>()
            };
            cyclic.Reference["x"] = cyclic.Reference;
            var exception = new CyclicExceptionDict
            {
                MyObjectDict = cyclic
            };

            // Act
            var result = new ExceptionPropertiesBag(new Exception());
            var destructurer = new ReflectionBasedDestructurer(10);
            destructurer.Destructure(exception, result, EmptyDestructurer());

            // Assert
            var myObject = (Dictionary<string, object>)result.GetResultDictionary()["MyObjectDict"];

            // exception.MyObjectDict["Reference"] is still regular dictionary
            var firstLevelDict = Assert.IsType<Dictionary<string, object>>(myObject["Reference"]);
            var id = firstLevelDict["$id"];
            Assert.Equal("1", id);

            // exception.MyObjectDict["Reference"]["x"] we notice that we are destructuring same dictionary
            var secondLevelDict = Assert.IsType<Dictionary<string, object>>(firstLevelDict["x"]);
            var refId = Assert.IsType<string>(secondLevelDict["$ref"]);
            Assert.Equal(id, refId);
        }

        [Fact]
        public void WhenObjectContainsCyclicReferencesInTask_ThenRecursiveDestructureIsImmediatelyStopped()
        {
            // Arrange
            var exception = new CyclicExceptionTask();
            var task = Task.FromException(exception);
            exception.Task = task;

            // Act
            var result = new ExceptionPropertiesBag(exception);
            var destructurer = CreateReflectionBasedDestructurer();
            destructurer.Destructure(exception, result, InnerDestructurer(destructurer));

            // Assert
            var resultsDictionary = result.GetResultDictionary();
            var destructuredTask = resultsDictionary[nameof(CyclicExceptionTask.Task)].Should().BeAssignableTo<IDictionary<string, object>>().Which;
            var destructuredCyclicException = destructuredTask.Should().ContainKey(nameof(Task.Exception))
                .WhichValue.Should().BeAssignableTo<IDictionary<string, object>>()
                .Which.Should().ContainKey(nameof(AggregateException.InnerExceptions))
                .WhichValue.Should().BeAssignableTo<IReadOnlyCollection<object>>()
                .Which.Should().ContainSingle()
                .Which.Should().BeAssignableTo<IDictionary<string, object>>().Which;
            destructuredCyclicException.Should().ContainKey(nameof(Exception.Message))
                .WhichValue.Should().BeOfType<string>()
                .Which.Should().Contain(nameof(CyclicExceptionTask));
            destructuredCyclicException.Should().ContainKey(nameof(CyclicExceptionTask.Task))
                .WhichValue.Should().BeAssignableTo<IDictionary<string, object>>()
                .Which.Should().ContainKey("$ref", "task was already destructured, so inner task should just contain ref");
        }

        [Fact]
        public void WhenDestruringArgumentException_ResultShouldBeEquivalentToArgumentExceptionDestructurer()
        {
            var exception = ThrowAndCatchException(() => throw new ArgumentException("MESSAGE", "paramName"));
            Test_ResultOfReflectionDestructurerShouldBeEquivalentToCustomOne(exception, new ArgumentExceptionDestructurer());
        }

        [Fact]
        public void CanDestructureObjectWithHiddenProperty()
        {
            var derived = new DerivedClass<int>
            {
                HiddenProperty = 123
            };
            var exception = new HiddenException("test", derived);

            var propertiesBag = new ExceptionPropertiesBag(exception);
            CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

            var properties = propertiesBag.GetResultDictionary();
            var derivedValue = properties[nameof(HiddenException.Info)] as IDictionary<string, object>;
            Assert.Equal(derived.HiddenProperty, derivedValue[nameof(DerivedClass<object>.HiddenProperty)]);
        }

        private static void Test_ResultOfReflectionDestructurerShouldBeEquivalentToCustomOne(
            Exception exception,
            IExceptionDestructurer customDestructurer)
        {
            // Arrange
            var reflectionBasedResult = new ExceptionPropertiesBag(exception);
            var customBasedResult = new ExceptionPropertiesBag(exception);
            var reflectionBasedDestructurer = CreateReflectionBasedDestructurer();

            // Act
            reflectionBasedDestructurer.Destructure(exception, reflectionBasedResult, InnerDestructurer(reflectionBasedDestructurer));
            customDestructurer.Destructure(exception, customBasedResult, InnerDestructurer(new ArgumentExceptionDestructurer()));

            // Assert
            var reflectionBasedDictionary = (Dictionary<string, object>)reflectionBasedResult.GetResultDictionary();
            var customBasedDictionary = (Dictionary<string, object>)customBasedResult.GetResultDictionary();

            reflectionBasedDictionary.Should().BeEquivalentTo(customBasedDictionary);
        }

        private static Func<Exception, IReadOnlyDictionary<string, object>> EmptyDestructurer() =>
            (ex) => new ExceptionPropertiesBag(ex).GetResultDictionary();

        private static Func<Exception, IReadOnlyDictionary<string, object>> InnerDestructurer(
            IExceptionDestructurer destructurer) =>
            (ex) =>
            {
                var resultsBag = new ExceptionPropertiesBag(ex);

                destructurer.Destructure(ex, resultsBag, InnerDestructurer(destructurer));

                return resultsBag.GetResultDictionary();
            };

        private static Exception ThrowAndCatchException(Action throwingAction)
        {
            try
            {
                throwingAction();
            }
            catch (Exception ex)
            {
                return ex;
            }

            Assert.True(false, $"{nameof(throwingAction)} did not throw");
            return null;
        }

        private static ReflectionBasedDestructurer CreateReflectionBasedDestructurer() =>
            new ReflectionBasedDestructurer(10);

        public class MyObject
        {
            public string Foo { get; set; }

            public MyObject Reference { get; set; }

            public MyObject Reference2 { get; set; }
        }

        public class CyclicException : Exception
        {
            public MyObject MyObject { get; set; }
        }

        public class MyObjectEnumerable : IEnumerable<MyObjectEnumerable>
        {
            public string Foo { get; set; }

            public MyObjectEnumerable Reference { get; set; }

            public IEnumerator<MyObjectEnumerable> GetEnumerator() =>
                new List<MyObjectEnumerable> { this.Reference }.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public class CyclicException2 : Exception
        {
            public MyObjectEnumerable MyObjectEnumerable { get; set; }
        }

        public class CyclicExceptionDict : Exception
        {
            public MyObjectDict MyObjectDict { get; set; }
        }

        public class CyclicExceptionTask : Exception
        {
            public Task Task { get; set; }
        }

        public class MyObjectDict
        {
            public string Foo { get; set; }

            public Dictionary<string, object> Reference { get; set; }
        }

        public class ExceptionWithTypeProperty : Exception
        {
            public int Type { get; set; }
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

            public string ExceptionProperty => throw new Exception();

            internal string InternalProperty { get; set; }

            protected string ProtectedProperty { get; set; }

#pragma warning disable IDE0052 // Remove unread private members
            private string PrivateProperty { get; set; }
#pragma warning restore IDE0052 // Remove unread private members

            public string this[int i] => "IndexerValue";
        }

        public class UriException : Exception
        {
            public UriException(string message, Uri uri)
                : base(message) =>
                this.Uri = uri;

            public Uri Uri { get; }
        }

        public class TaskException : Exception
        {
            public TaskException(string message, Task task)
                : base(message) =>
                this.Task = task;

            public Task Task { get; }
        }

        public class RecursiveNode
        {
            public string Name { get; set; }

            public RecursiveNode Child { get; set; }
        }

        public class RecursiveException : Exception
        {
            public RecursiveNode Node { get; set; }
        }

        [Serializable]
#pragma warning disable SA1201 // Elements should appear in the correct order
        internal struct TestStruct
#pragma warning restore SA1201 // Elements should appear in the correct order
        {
            public int ValueType { get; set; }

            public string ReferenceType { get; set; }
        }

        [Serializable]
        internal class TestClass
        {
            public int ValueType { get; set; }

            public string ReferenceType { get; set; }
        }

        internal class BaseClass
        {
            public virtual int HiddenProperty { get; set; }
        }

        internal class DerivedClass<T> : BaseClass
        {
            public new T HiddenProperty { get; set; }
        }

        internal class HiddenException : Exception
        {
            public HiddenException(string message, object info)
                : base(message)
            {
                Info = info;
            }

            public object Info { get; set; }
        }
    }
}
