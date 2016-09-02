<h1>
    <img src="https://raw.githubusercontent.com/RehanSaeed/Serilog.Exceptions/master/Images/Serilog%20Community%20256x256.png" alt="Serilog.Exceptions Logo" width="30px" height="30px" /> <a href="https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate">Serilog.Exceptions</a>
</h1>
Serilog.Exceptions is an add-on to [Serilog](http://serilog.net/) to log exception details and custom properties that are not output in Exception.ToString().

[![Build status](https://ci.appveyor.com/api/projects/status/7ijbthe6iig9phn6?svg=true)](https://ci.appveyor.com/project/RehanSaeed/serilog-exceptions)
[![NuGet](https://img.shields.io/nuget/v/Serilog.Exceptions.svg)](https://www.nuget.org/packages/Serilog.Exceptions/)
[![MyGet](https://img.shields.io/myget/serilog-exceptions/v/Serilog.Exceptions.svg)](http://myget.org/gallery/serilog-exceptions)

## What Does It Do?

Your JSON logs will now be supplemented with detailed exception information and even custom exception properties. Here is an example of what happens when you log a DbEntityValidationException from EntityFramework (This exception is notorious for having deeply nested custom properties which are not included in the `.ToString()`).

```
try
{
    ...
}
catch (DbEntityValidationException exception)
{
    logger.Error(exception, "Hello World");
}
```

The code above logs the following:

```
{
  "Timestamp": "2015-12-07T12:26:24.0557671+00:00",
  "Level": "Error",
  "MessageTemplate": "Hello World",
  "RenderedMessage": "Hello World",
  "Exception": "System.Data.Entity.Validation.DbEntityValidationException: Message",
  "Properties": {
    "ExceptionDetail": {
      "EntityValidationErrors": [
        {
          "Entry": null,
          "ValidationErrors": [
            {
              "PropertyName": "PropertyName",
              "ErrorMessage": "PropertyName is Required.",
              "Type": "System.Data.Entity.Validation.DbValidationError"
            }
          ],
          "IsValid": false,
          "Type": "System.Data.Entity.Validation.DbEntityValidationResult"
        }
      ],
      "Message": "Validation failed for one or more entities. See 'EntityValidationErrors' property for more details.",
      "Data": {},
      "InnerException": null,
      "TargetSite": null,
      "StackTrace": null,
      "HelpLink": null,
      "Source": null,
      "HResult": -2146232032,
      "Type": "System.Data.Entity.Validation.DbEntityValidationException"
    },
    "Source": "418169ff-e65f-456e-8b0d-42a0973c3577"
  }
}
```

## Getting Started

Add the [Serilog.Exceptions](https://www.nuget.org/packages/Serilog.Exceptions/) NuGet package to your project using the NuGet Package Manager or run the following command in the Package Console Window:

```
Install-Package Serilog.Exceptions
```

When setting up your logger, add the `With.ExceptionDetails()` line like so:

```
using Serilog;
using Serilog.Exceptions;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithExceptionDetails()
    .WriteTo.Sink(new RollingFileSink(
        @"C:\logs",
        new JsonFormatter(renderMessage: true))
    .CreateLogger();
```

## Performance

This library has custom code to deal with extra properties on most common exception types and only falls back to using reflection to get the extra information if the exception is not supported by Serilog.Exceptions internally.

## Custom Exception Destructurers

You may want to add support for destructuring your own exceptions without relying on reflection. To do this, create your own destructuring class implementing `ExceptionDestructurer` (You can take a look at [this](https://github.com/RehanSaeed/Serilog.Exceptions/blob/master/Source/Serilog.Exceptions/Destructurers/ArgumentExceptionDestructurer.cs) for `ArgumentException`), then simply add it like so:

```
using Serilog;
using Serilog.Exceptions;

var exceptionDestructurers = new List<IExceptionDestructurer>();
exceptionDestructurers.AddRange(ExceptionEnricher.DefaultDestructurers);  // Add built in destructurers.
exceptionDestructurers.Add(new MyCustomExceptionDestructurer());          // Add your custom destructurer.

ILogger logger = new LoggerConfiguration()
    .Enrich.WithExceptionDetails(exceptionDestructurers)
    .WriteTo.Sink(new RollingFileSink(
        @"C:\logs",
        new JsonFormatter(renderMessage: true))
    .CreateLogger();
```

If you write a destructurer that is not included in this project (even for a third party library), please contribute it.

## Contributions

The original code was taken from [here](https://groups.google.com/forum/#!searchin/getseq/enhance%2420exception/getseq/rsAL4u3JpLM/PrszbPbtEb0J) and then improved for better performance and to support more exception types without using reflection by [Muhammad Rehan Saeed](http://rehansaeed.com).

If you have created any custom destructurers, please contribute them even if they are for third party libraries. If they are for exceptions from a third party library e.g. EntityFramework, please contribute a Serilog.Exceptions.EntityFramework project containing all the destructurers for the all the exceptions in EntityFramework.
