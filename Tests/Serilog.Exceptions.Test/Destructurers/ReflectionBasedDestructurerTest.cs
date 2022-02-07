namespace Serilog.Exceptions.Test.Destructurers;

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
        Assert.DoesNotContain(properties, x => string.Equals(x.Key, "InternalProperty", StringComparison.Ordinal));
        Assert.DoesNotContain(properties, x => string.Equals(x.Key, "ProtectedProperty", StringComparison.Ordinal));
        Assert.DoesNotContain(properties, x => string.Equals(x.Key, "PrivateProperty", StringComparison.Ordinal));
        Assert.Equal("MessageValue", properties[nameof(TestException.Message)]);
#if NET461 || NET472
            Assert.StartsWith("Void DestructureComplexException_EachTypeOfPropertyIsDestructuredAsExpected(", properties[nameof(TestException.TargetSite)].ToString());
#endif
        Assert.NotEmpty(properties[nameof(TestException.StackTrace)]?.ToString());
        Assert.Equal("Serilog.Exceptions.Test", properties[nameof(TestException.Source)]);
        Assert.Equal(-2146233088, properties[nameof(TestException.HResult)]);
        Assert.Contains(typeof(TestException).FullName, properties["Type"]?.ToString(), StringComparison.Ordinal);
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
                    { "UriDataItem", new Uri(uriValue) },
                },
        };

        var propertiesBag = new ExceptionPropertiesBag(exception);
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        var properties = propertiesBag.GetResultDictionary();
        var data = (IDictionary?)properties[nameof(Exception.Data)];
        var uriDataValue = data?["UriDataItem"];
        Assert.IsType<string>(uriDataValue);
        Assert.Equal(uriValue, uriDataValue);
    }

    [Fact]
    public async Task CanDestructureTaskAsync()
    {
        using var cancellationTokenSource = new CancellationTokenSource(0);

        TaskCanceledException exception;
        try
        {
            await Task.Delay(1000, cancellationTokenSource.Token).ConfigureAwait(false);
            Assert.True(false, "TaskCanceledException was not thrown.");
            return;
        }
        catch (TaskCanceledException taskCancelledException)
        {
            exception = taskCancelledException;
        }

        var propertiesBag = new ExceptionPropertiesBag(exception);
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        var properties = propertiesBag.GetResultDictionary();
        var destructuredTaskObject = (IDictionary?)properties[nameof(TaskCanceledException.Task)];
        var destructuredTaskProperties = Assert.IsAssignableFrom<IDictionary<string, object>>(destructuredTaskObject);
        destructuredTaskProperties.Should().ContainKey(nameof(Task.Id));
        destructuredTaskProperties.Should().ContainKey(nameof(Task.Status))
            .WhoseValue.Should().BeOfType<string>()
            .Which.Should().Be(nameof(TaskStatus.Canceled));
        destructuredTaskProperties.Should().ContainKey(nameof(Task.CreationOptions))
            .WhoseValue.Should().BeOfType<string>()
            .Which.Should().Contain(nameof(TaskCreationOptions.None));
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
        var destructuredTaskObject = (IDictionary?)properties[nameof(TaskCanceledException.Task)];
        var destructuredTaskProperties = Assert.IsAssignableFrom<IDictionary<string, object>>(destructuredTaskObject);
        destructuredTaskProperties.Should().ContainKey(nameof(Task.Id));
        destructuredTaskProperties.Should().ContainKey(nameof(Task.Status))
            .WhoseValue.Should().BeOfType<string>()
            .Which.Should().Be(nameof(TaskStatus.Faulted));
        destructuredTaskProperties.Should().ContainKey(nameof(Task.CreationOptions))
            .WhoseValue.Should().BeOfType<string>()
            .Which.Should().Be(nameof(TaskCreationOptions.None));
        var taskFirstLevelExceptionDictionary = destructuredTaskProperties.Should().ContainKey(nameof(Task.Exception))
            .WhoseValue.Should().BeAssignableTo<IDictionary<string, object>>()
            .Which;
        taskFirstLevelExceptionDictionary.Should().ContainKey("Message")
            .WhoseValue.Should().BeOfType<string>()
            .Which.Should().Contain("One or more errors occurred.", "task's first level exception is aggregate exception");
        taskFirstLevelExceptionDictionary.Should().ContainKey("InnerExceptions")
            .WhoseValue.Should().BeAssignableTo<IReadOnlyCollection<object>>()
            .Which.Should().ContainSingle()
            .Which.Should().BeAssignableTo<IDictionary<string, object>>()
            .Which.Should().ContainKey("Message")
            .WhoseValue.Should().BeOfType<string>()
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
            ReferenceType = "ABC",
        };
        var propertiesBag = new ExceptionPropertiesBag(exception);

        // Act
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        // Assert
        var properties = propertiesBag.GetResultDictionary();
        var data = (IDictionary?)properties[nameof(Exception.Data)];
        var testStructDataValue = data?["data"];
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
            ReferenceType = "ABC",
        };
        var propertiesBag = new ExceptionPropertiesBag(exception);

        // Act
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        // Assert
        var properties = propertiesBag.GetResultDictionary();
        var data = (IDictionary?)properties[nameof(Exception.Data)];
        var testStructDataValue = data?["data"];
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
                        Name = "CHILD 2",
                    },
                },
            },
        };
        var destructurer = new ReflectionBasedDestructurer(1);

        // Act
        var propertiesBag = new ExceptionPropertiesBag(exception);
        destructurer.Destructure(exception, propertiesBag, EmptyDestructurer());

        // Assert
        // Parent is depth 1
        // First child is depth 2
        var properties = propertiesBag.GetResultDictionary();
        var parent = (IDictionary<string, object?>?)properties[nameof(RecursiveException.Node)];
        Assert.Equal("PARENT", parent?[nameof(RecursiveNode.Name)]);
        Assert.IsType<RecursiveNode>(parent?[nameof(RecursiveNode.Child)]);
    }

    [Fact]
    public void ExceptionWithTypeProperty_StillContainsType_JustWithDollarAsPrefixInLabel()
    {
        var exceptionWithTypeProperty = new TypePropertyException() { Type = 13 };
        Test_LoggedExceptionContainsProperty(exceptionWithTypeProperty, "$Type", $"Serilog.Exceptions.Test.Destructurers.{nameof(ReflectionBasedDestructurerTest)}+{nameof(TypePropertyException)}");
    }

    [Fact]
    public void WhenObjectContainsCyclicReferences_ThenNoStackoverflowExceptionIsThrown()
    {
        // Arrange
        var exception = new CyclicException
        {
            MyObject = new MyObject(),
        };
        exception.MyObject.Foo = "bar";
        exception.MyObject.Reference = exception.MyObject;
        exception.MyObject.Reference2 = exception.MyObject;

        // Act
        var result = new ExceptionPropertiesBag(new Exception());
        var destructurer = new ReflectionBasedDestructurer(10);
        destructurer.Destructure(exception, result, EmptyDestructurer());

        // Assert
        var myObject = (Dictionary<string, object?>?)result.GetResultDictionary()["MyObject"];

        Assert.Equal("bar", myObject?["Foo"]);
        var reference1 = (Dictionary<string, object?>?)myObject?["Reference"];
        Assert.Equal(myObject?["$id"], reference1?["$ref"]);
        var reference2 = (Dictionary<string, object?>?)myObject?["Reference2"];
        Assert.Equal(myObject?["$id"], reference2?["$ref"]);
        Assert.Equal("1", myObject?["$id"]);
    }

    [Fact]
    public void WhenObjectContainsCyclicReferencesInList_ThenRecursiveDestructureIsImmediatelyStopped()
    {
        // Arrange
        var cyclic = new MyObjectCollection
        {
            Foo = "Cyclic",
        };
        cyclic.Reference = cyclic;
        var exception = new Cyclic2Exception
        {
            MyObjectCollection = new MyObjectCollection(),
        };
        exception.MyObjectCollection.Foo = "bar";
        exception.MyObjectCollection.Reference = cyclic;

        // Act
        var result = new ExceptionPropertiesBag(new Exception());
        var destructurer = new ReflectionBasedDestructurer(10);
        destructurer.Destructure(exception, result, EmptyDestructurer());

        // Assert
        var myObject = (List<object?>?)result.GetResultDictionary()[nameof(Cyclic2Exception.MyObjectCollection)];

        // exception.MyObjectCollection[0] is still list
        var firstLevelList = Assert.IsType<List<object>>(myObject?[0]);

        // exception.MyObjectCollection[0][0] we notice that we would again destructure "cyclic"
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
            Reference = new Dictionary<string, object>(),
        };
        cyclic.Reference["x"] = cyclic.Reference;
        var exception = new CyclicDictException
        {
            MyObjectDict = cyclic,
        };

        // Act
        var result = new ExceptionPropertiesBag(new Exception());
        var destructurer = new ReflectionBasedDestructurer(10);
        destructurer.Destructure(exception, result, EmptyDestructurer());

        // Assert
        var myObject = (Dictionary<string, object?>?)result.GetResultDictionary()["MyObjectDict"];

        // exception.MyObjectDict["Reference"] is still regular dictionary
        var firstLevelDict = Assert.IsType<Dictionary<string, object>>(myObject?["Reference"]);
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
        var exception = new CyclicTaskException();
        var task = Task.FromException(exception);
        exception.Task = task;

        // Act
        var result = new ExceptionPropertiesBag(exception);
        var destructurer = CreateReflectionBasedDestructurer();
        destructurer.Destructure(exception, result, InnerDestructurer(destructurer));

        // Assert
        var resultsDictionary = result.GetResultDictionary();
        var destructuredTask = resultsDictionary[nameof(CyclicTaskException.Task)].Should().BeAssignableTo<IDictionary<string, object>>().Which;
        var destructuredCyclicException = destructuredTask.Should().ContainKey(nameof(Task.Exception))
            .WhoseValue.Should().BeAssignableTo<IDictionary<string, object>>()
            .Which.Should().ContainKey(nameof(AggregateException.InnerExceptions))
            .WhoseValue.Should().BeAssignableTo<IReadOnlyCollection<object>>()
            .Which.Should().ContainSingle()
            .Which.Should().BeAssignableTo<IDictionary<string, object>>().Which;
        destructuredCyclicException.Should().ContainKey(nameof(Exception.Message))
            .WhoseValue.Should().BeOfType<string>()
            .Which.Should().Contain(nameof(CyclicTaskException));
        destructuredCyclicException.Should().ContainKey(nameof(CyclicTaskException.Task))
            .WhoseValue.Should().BeAssignableTo<IDictionary<string, object>>()
            .Which.Should().ContainKey("$ref", "task was already destructured, so inner task should just contain ref");
    }

    [Fact]
    public void WhenDestruringArgumentException_ResultShouldBeEquivalentToArgumentExceptionDestructurer()
    {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
        var exception = ThrowAndCatchException(() => throw new ArgumentException("MESSAGE", "paramName"));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        Test_ResultOfReflectionDestructurerShouldBeEquivalentToCustomOne(exception, new ArgumentExceptionDestructurer());
    }

    [Fact]
    public void CanDestructureObjectWithHiddenProperty()
    {
        var derived = new DerivedClass<int>
        {
            HiddenProperty = 123,
        };
        var baseClass = (BaseClass)derived;
        baseClass.HiddenProperty = 456;
        var exception = new HiddenException("test", derived);

        var propertiesBag = new ExceptionPropertiesBag(exception);
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        var properties = propertiesBag.GetResultDictionary();
        var info = properties[nameof(HiddenException.Info)] as IDictionary<string, object>;
        Assert.Equal(derived.HiddenProperty, info?[nameof(DerivedClass<object>.HiddenProperty)]);
        Assert.Equal(baseClass.HiddenProperty, info?[$"{nameof(BaseClass)}.{nameof(BaseClass.HiddenProperty)}"]);
    }

    [Fact]
    public void CanDestructureObjectWithRedefinedProperty()
    {
        var exception = new TestExceptionClassWithNewDefinition() { PublicProperty = 20 };

        var propertiesBag = new ExceptionPropertiesBag(exception);
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        var properties = propertiesBag.GetResultDictionary();
        var info = properties[nameof(TestExceptionClassWithNewDefinition.PublicProperty)];
    }

    [Fact]
    public void CanDestructureObjectWithDataWithRedefinedProperty()
    {
        var exception = new RecursiveException
        {
            Node = new RecursiveNodeWithRedefinedProperty
            {
                Name = 123,
            },
        };

        var propertiesBag = new ExceptionPropertiesBag(exception);
        CreateReflectionBasedDestructurer().Destructure(exception, propertiesBag, EmptyDestructurer());

        var properties = propertiesBag.GetResultDictionary();
        var parent = (IDictionary<string, object?>?)properties[nameof(RecursiveException.Node)];
        Assert.Equal(123, parent?[nameof(RecursiveNode.Name)]);
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

    private static Func<Exception, IReadOnlyDictionary<string, object?>> EmptyDestructurer() =>
        (ex) => new ExceptionPropertiesBag(ex).GetResultDictionary();

    private static Func<Exception, IReadOnlyDictionary<string, object?>?> InnerDestructurer(
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
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return ex;
        }

        Assert.True(false, $"{nameof(throwingAction)} did not throw");
        return null!; // We should never reach this line.
    }

    private static ReflectionBasedDestructurer CreateReflectionBasedDestructurer() =>
        new(10);

    public class MyObject
    {
        public string? Foo { get; set; }

        public MyObject? Reference { get; set; }

        public MyObject? Reference2 { get; set; }
    }

    public class CyclicException : Exception
    {
        public MyObject? MyObject { get; set; }
    }

    public class MyObjectCollection : IEnumerable<MyObjectCollection>
    {
        public string? Foo { get; set; }

        public MyObjectCollection? Reference { get; set; }

        public IEnumerator<MyObjectCollection> GetEnumerator() =>
            new List<MyObjectCollection?> { this.Reference }.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public class Cyclic2Exception : Exception
    {
        public MyObjectCollection? MyObjectCollection { get; set; }
    }

    public class CyclicDictException : Exception
    {
        public MyObjectDict? MyObjectDict { get; set; }
    }

    public class CyclicTaskException : Exception
    {
        public Task? Task { get; set; }
    }

    public class MyObjectDict
    {
        public string? Foo { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public Dictionary<string, object>? Reference { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class TypePropertyException : Exception
    {
#pragma warning disable CA1721 // Property names should not match get methods
        public int? Type { get; set; }
#pragma warning restore CA1721 // Property names should not match get methods
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

        public static string? StaticProperty { get; set; }

        public string PublicProperty { get; set; }

#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
#pragma warning disable CA1822 // Member does not access instance data and can be marked as static
        public string ExceptionProperty => throw new Exception();
#pragma warning restore CA1822 // Member does not access instance data and can be marked as static
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations

        internal string InternalProperty { get; set; }

        protected string ProtectedProperty { get; set; }

#pragma warning disable IDE0052 // Remove unread private members
        private string PrivateProperty { get; set; }
#pragma warning restore IDE0052 // Remove unread private members

#pragma warning disable CA1822 // Member does not access instance data and can be marked as static
        public string this[int i] => "IndexerValue";
#pragma warning restore CA1822 // Member does not access instance data and can be marked as static
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
        public string? Name { get; set; }

        public RecursiveNode? Child { get; set; }
    }

    public class RecursiveNodeWithRedefinedProperty : RecursiveNode
    {
        public new int Name { get; set; }
    }

    public class RecursiveException : Exception
    {
        public RecursiveNode? Node { get; set; }
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

        public string? ReferenceType { get; set; }
    }

    internal class TestExceptionClassWithNewDefinition : TestException
    {
        public new int PublicProperty { get; set; }
    }

    internal class BaseClass
    {
        public virtual int HiddenProperty { get; set; }
    }

    internal class DerivedClass<T> : BaseClass
    {
        public new T? HiddenProperty { get; set; }
    }

#pragma warning disable CA1064 // Exceptions should be public
    internal class HiddenException : Exception
#pragma warning restore CA1064 // Exceptions should be public
    {
        public HiddenException(string message, object info)
            : base(message) =>
            this.Info = info;

        public object Info { get; set; }
    }
}
