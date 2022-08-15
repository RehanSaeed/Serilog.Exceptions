![Serilog.Exceptions Banner](https://github.com/RehanSaeed/Serilog.Exceptions/blob/main/Images/Banner.png)

[![Serilog.Exceptions NuGet Package](https://img.shields.io/nuget/v/Serilog.Exceptions.svg)](https://www.nuget.org/packages/Serilog.Exceptions/) [![Serilog.Exceptions package in serilog-exceptions feed in Azure Artifacts](https://feeds.dev.azure.com/serilog-exceptions/_apis/public/Packaging/Feeds/8479813c-da6b-4677-b40d-78df8725dc9c/Packages/212043f6-5fe5-4c79-949e-162156b89894/Badge)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_packaging?_a=package&feed=8479813c-da6b-4677-b40d-78df8725dc9c&package=212043f6-5fe5-4c79-949e-162156b89894&preferRelease=true) [![Serilog.Exceptions NuGet Package Downloads](https://img.shields.io/nuget/dt/Serilog.Exceptions)](https://www.nuget.org/packages/Serilog.Exceptions) [![Twitter URL](https://img.shields.io/twitter/url/http/shields.io.svg?style=social)](https://twitter.com/RehanSaeedUK) [![Twitter Follow](https://img.shields.io/twitter/follow/rehansaeeduk.svg?style=social&label=Follow)](https://twitter.com/RehanSaeedUK)

Serilog.Exceptions is an add-on to [Serilog](https://serilog.net) to log exception details and custom properties that are not output in `Exception.ToString()`.

## What Does It Do?

Your JSON logs will now be supplemented with detailed exception information and even custom exception properties. Here is an example of what happens when you log a `DbEntityValidationException` from EntityFramework (This exception is notorious for having deeply nested custom properties which are not included in the `.ToString()`).

```csharp
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

```json
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

```powershell
dotnet add package Serilog.Exceptions
```

When setting up your logger, add the `WithExceptionDetails()` line like so:

```csharp
using Serilog;
using Serilog.Exceptions;

ILogger logger = new LoggerConfiguration()
    .Enrich.WithExceptionDetails()
    .WriteTo.RollingFile(
        new JsonFormatter(renderMessage: true), 
        @"C:\logs\log-{Date}.txt")    
    .CreateLogger();
```

Make sure that the sink's formatter outputs enriched properties. `Serilog.Sinks.Console` and many more do not do that by default. You may need to add `{Properties:j}` to your sink's format template. For example, configuration for console sink may look like that:

```csharp
.WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception} {Properties:j}")
```

### JSON `appSettings.json` configuration

Alternatively to fluent configuration setting can be stored in application configuration using [_Serilog.Settings.Configuration_](https://github.com/serilog/serilog-settings-configuration):
```json
{
  "Serilog": {
    "Using": [ "Serilog.Exceptions" ],
    "Enrich": [ "WithExceptionDetails" ],
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

## Performance

This library has custom code to deal with extra properties on most common exception types and only falls back to using reflection to get the extra information if the exception is not supported by Serilog.Exceptions internally. Reflection overhead is present but minimal, because all the expensive relection-based operations are done only once per exception-type.

## Additional Destructurers

### Serilog.Exceptions.SqlServer

[![Serilog.Exceptions.SqlServer NuGet Package](https://img.shields.io/nuget/v/Serilog.Exceptions.SqlServer.svg)](https://www.nuget.org/packages/Serilog.Exceptions.SqlServer/) [![Serilog.Exceptions.SqlServer package in serilog-exceptions feed in Azure Artifacts](https://feeds.dev.azure.com/serilog-exceptions/_apis/public/Packaging/Feeds/8479813c-da6b-4677-b40d-78df8725dc9c/Packages/67be830c-2c0f-4df8-be30-771d817b382f/Badge)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_packaging?_a=package&feed=8479813c-da6b-4677-b40d-78df8725dc9c&package=67be830c-2c0f-4df8-be30-771d817b382f&preferRelease=true) [![Serilog.Exceptions.SqlServer NuGet Package Downloads](https://img.shields.io/nuget/dt/Serilog.Exceptions.SqlServer)](https://www.nuget.org/packages/Serilog.Exceptions.SqlServer)

Add the [Serilog.Exceptions.SqlServer](https://www.nuget.org/packages/Serilog.Exceptions.SqlServer/) NuGet package to your project to avoid the reflection based destructurer for `SqlException` when using [System.Data.SqlClient](https://www.nuget.org/packages/System.Data.SqlClient/):

```
Install-Package Serilog.Exceptions.SqlServer
```

Add the `SqlExceptionDestructurer` during setup:
```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new SqlExceptionDestructurer() }))
```

### Serilog.Exceptions.MsSqlServer

[![Serilog.Exceptions.MsSqlServer NuGet Package](https://img.shields.io/nuget/v/Serilog.Exceptions.MsSqlServer.svg)](https://www.nuget.org/packages/Serilog.Exceptions.MsSqlServer/) 
[![Serilog.Exceptions.MsSqlServer package in serilog-exceptions feed in Azure Artifacts](https://feeds.dev.azure.com/serilog-exceptions/_apis/public/Packaging/Feeds/8479813c-da6b-4677-b40d-78df8725dc9c/Packages/dce98084-312a-4939-b879-07bc25734572/Badge)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_packaging?_a=package&feed=8479813c-da6b-4677-b40d-78df8725dc9c&package=dce98084-312a-4939-b879-07bc25734572&preferRelease=true) [![Serilog.Exceptions.MsSqlServer NuGet Package Downloads](https://img.shields.io/nuget/dt/Serilog.Exceptions.MsSqlServer)](https://www.nuget.org/packages/Serilog.Exceptions.MsSqlServer)

Add the [Serilog.Exceptions.MsSqlServer](https://www.nuget.org/packages/Serilog.Exceptions.MsSqlServer/) NuGet package to your project to avoid the reflection based destructurer for `SqlException` when using [Microsoft.Data.SqlClient](https://www.nuget.org/packages/Microsoft.Data.SqlClient/):

```
Install-Package Serilog.Exceptions.MsSqlServer
```

Add the `SqlExceptionDestructurer` during setup:
```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new SqlExceptionDestructurer() }))
```

### Serilog.Exceptions.EntityFrameworkCore

[![Serilog.Exceptions.EntityFrameworkCore NuGet Package](https://img.shields.io/nuget/v/Serilog.Exceptions.EntityFrameworkCore.svg)](https://www.nuget.org/packages/Serilog.Exceptions.EntityFrameworkCore/) [![Serilog.Exceptions.EntityFrameworkCore package in serilog-exceptions feed in Azure Artifacts](https://feeds.dev.azure.com/serilog-exceptions/_apis/public/Packaging/Feeds/8479813c-da6b-4677-b40d-78df8725dc9c/Packages/ee2cd6f8-4c93-4774-9398-23c49ba41928/Badge)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_packaging?_a=package&feed=8479813c-da6b-4677-b40d-78df8725dc9c&package=ee2cd6f8-4c93-4774-9398-23c49ba41928&preferRelease=true) [![Serilog.Exceptions.EntityFrameworkCore NuGet Package Downloads](https://img.shields.io/nuget/dt/Serilog.Exceptions.EntityFrameworkCore)](https://www.nuget.org/packages/Serilog.Exceptions.EntityFrameworkCore)

> **WARNING**: In versions of Serilog.Exceptions older than [8.0.0](https://github.com/RehanSaeed/Serilog.Exceptions/releases/tag/8.0.0), if you are using EntityFrameworkCore with Serilog.Exceptions you must add this, otherwise in certain cases your entire database will be logged! This is because the exceptions in Entity Framework Core have properties that link to the entire database schema in them (See [#100](https://github.com/RehanSaeed/Serilog.Exceptions/issues/100), [aspnet/EntityFrameworkCore#15214](https://github.com/aspnet/EntityFrameworkCore/issues/15214)). Newer versions of Serilog.Exceptions avoids this issue by preventing the destructure of properties that implement IQueryable preventing their execution.

Add the [Serilog.Exceptions.EntityFrameworkCore](https://www.nuget.org/packages/Serilog.Exceptions.EntityFrameworkCore/) NuGet package to your project when using EntityFrameworkCore in your project

```
Install-Package Serilog.Exceptions.EntityFrameworkCore
```

Add the `DbUpdateExceptionDestructurer` during setup:
```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() }))
```

### Serilog.Exceptions.Refit

[![Serilog.Exceptions.Refit NuGet Package](https://img.shields.io/nuget/v/Serilog.Exceptions.Refit.svg)](https://www.nuget.org/packages/Serilog.Exceptions.Refit/) 
[![Serilog.Exceptions.Refit package in serilog-exceptions feed in Azure Artifacts](https://feeds.dev.azure.com/serilog-exceptions/_apis/public/Packaging/Feeds/8479813c-da6b-4677-b40d-78df8725dc9c/Packages/dce98084-312a-4939-b879-07bc25734572/Badge)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_packaging?_a=package&feed=8479813c-da6b-4677-b40d-78df8725dc9c&package=dce98084-312a-4939-b879-07bc25734572&preferRelease=true) [![Serilog.Exceptions.Refit NuGet Package Downloads](https://img.shields.io/nuget/dt/Serilog.Exceptions.Refit)](https://www.nuget.org/packages/Serilog.Exceptions.Refit)

Add the [Serilog.Exceptions.Refit](https://www.nuget.org/packages/Serilog.Exceptions.Refit/) NuGet package to your project to provide detailed logging for the `ApiException` when using [Refit](https://www.nuget.org/packages/Refit/):

```
Install-Package Serilog.Exceptions.Refit
```

Add the `ApiExceptionDestructurer` during setup:
```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new ApiExceptionDestructurer() }))
```

Depending on your Serilog setup, common `System.Exception` properties may already be logged. To omit the logging of these properties, use the overloaded
constructor as follows:

```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new ApiExceptionDestructurer(destructureCommonExceptionProperties: false) }))
```

The default configuration logs the following properties of an `ApiException`:

- `Uri`
- `StatusCode`

In addition, the `ApiException.Content` property can be logged with the following setup:

```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new ApiExceptionDestructurer(destructureHttpContent: true) }))
```

Be careful with this option as the HTTP body could be very large and/or contain sensitive information.

### Serilog.Exceptions.Grpc

[![Serilog.Exceptions.Grpc NuGet Package](https://img.shields.io/nuget/v/Serilog.Exceptions.Grpc.svg)](https://www.nuget.org/packages/Serilog.Exceptions.Grpc/)  
[![Serilog.Exceptions.Grpc NuGet Package Downloads](https://img.shields.io/nuget/dt/Serilog.Exceptions.Grpc)](https://www.nuget.org/packages/Serilog.Exceptions.Grpc)

Add the [Serilog.Exceptions.Grpc](https://www.nuget.org/packages/Serilog.Exceptions.Grpc/) NuGet package to your project to avoid the reflection based destructurer for `RpcException` when using [Grpc.Net.Client](https://www.nuget.org/packages/Grpc.Net.Client/):

```
Install-Package Serilog.Exceptions.Grpc
```

Add the `RpcExceptionDestructurer` during setup:
```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new RpcExceptionDestructurer() }))
```

## Custom Exception Destructurers

You may want to add support for destructuring your own exceptions without relying on reflection. To do this, create your own destructuring class implementing `ExceptionDestructurer` (You can take a look at [this](https://github.com/RehanSaeed/Serilog.Exceptions/blob/main/Source/Serilog.Exceptions/Destructurers/ArgumentExceptionDestructurer.cs) for `ArgumentException`), then simply add it like so:

```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithDestructurers(new[] { new MyCustomExceptionDestructurer() }))
```

If you write a destructurer that is not included in this project (even for a third party library), please contribute it.

## Additional configuration

You can configure some additional properties of destructuring process, by passing custom destructuring options during setup:

```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
    .WithDefaultDestructurers()
    .WithRootName("Exception"))
```

Currently following options are supported:

- `RootName`: The property name which will hold destructured exception, `ExceptionDetail` by default.
- `Filter`: The object implementing `IExceptionPropertyFilter` that will have a chance to filter properties just before they are put in destructured exception object. Go to "Filtering properties" section for details.
- `DestructuringDepth`: The maximum depth of reflection based recursive destructuring process.
- `ReflectionBasedDestructurer`: Reflection based destructurer is enabled by default, but can be disabled in case you want to have complete control over destructuring process. You will have to register destructurers for all exceptions explicitly.

## Filtering properties

You may want to skip some properties of all or part your exception classes without directly creating or modifying custom destructurers. Serilog.Exceptions supports this functionality using a filter.

Most typical use case is the need to skip `StackTrace` and `TargetSite`. Serilog is already reporting them so you may want Serilog.Exceptions to skip them to save space and processing time. To do that you just need to modify a line in configuration:

```csharp
.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithFilter(someFilter));
```

Filtering for other scenarios is also supported:

- Use `WithIgnoreStackTraceAndTargetSiteExceptionFilter` if you need to filter some other set of named properties
- Implement custom `IExceptionPropertyFilter` if you need some different filtering logic
- Use `CompositeExceptionPropertyFilter` to combine multiple filters

## Continuous Integration

| Name            | Operating System      | Status | History |
| :---            | :---                  | :---   | :---    |
| Azure Pipelines | Ubuntu                | [![Azure Pipelines Ubuntu Build Status](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_apis/build/status/RehanSaeed.Serilog.Exceptions?branchName=main&stageName=Build&jobName=Build&configuration=Build%20Linux)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_build/latest?definitionId=1&branchName=main) |
| Azure Pipelines | Mac                   | [![Azure Pipelines Mac Build Status](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_apis/build/status/RehanSaeed.Serilog.Exceptions?branchName=main&stageName=Build&jobName=Build&configuration=Build%20Mac)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_build/latest?definitionId=1&branchName=main) |
| Azure Pipelines | Windows               | [![Azure Pipelines Windows Build Status](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_apis/build/status/RehanSaeed.Serilog.Exceptions?branchName=main&stageName=Build&jobName=Build&configuration=Build%20Windows)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_build/latest?definitionId=1&branchName=main) |
| Azure Pipelines | Overall               | [![Azure Pipelines Overall Build Status](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_apis/build/status/RehanSaeed.Serilog.Exceptions?branchName=main)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_build/latest?definitionId=1&branchName=main) | [![Azure DevOps Build History](https://buildstats.info/azurepipelines/chart/serilog-exceptions/Serilog.Exceptions/1?branch=main&includeBuildsFromPullRequest=false)](https://dev.azure.com/serilog-exceptions/Serilog.Exceptions/_build/latest?definitionId=1&branchName=main) |
| GitHub Actions  | Ubuntu, Mac & Windows | [![GitHub Actions Status](https://github.com/RehanSaeed/Serilog.Exceptions/workflows/Build/badge.svg?branch=main)](https://github.com/RehanSaeed/Serilog.Exceptions/actions) | [![GitHub Actions Build History](https://buildstats.info/github/chart/RehanSaeed/Serilog.Exceptions?branch=main&includeBuildsFromPullRequest=false)](https://github.com/RehanSaeed/Serilog.Exceptions/actions) |
| AppVeyor        | Ubuntu, Mac & Windows | [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/7ijbthe6iig9phn6/branch/main?svg=true)](https://ci.appveyor.com/project/RehanSaeed/serilog-exceptions/branch/main) | [![AppVeyor Build History](https://buildstats.info/appveyor/chart/RehanSaeed/serilog-exceptions?branch=main&includeBuildsFromPullRequest=false)](https://ci.appveyor.com/project/RehanSaeed/serilog-exceptions) |

## Contributions and Thanks

Please view the [contributing guide](https://github.com/RehanSaeed/Serilog.Exceptions/blob/main/.github/CONTRIBUTING.md) for more information.

- [304NotModified](https://github.com/304NotModified) - Added Markdown syntax highlighting.
- [joelweiss](https://github.com/joelweiss) - Added Entity Framework Core destructurers.
- [krajek](https://github.com/krajek) & [JeroenDragt](https://github.com/JeroenDragt) - For adding filters to help ignore exception properties you don't want logged.
- [krajek](https://github.com/krajek) - For helping with cyclic dependencies when using the reflection destructurer.
- [mraming](https://github.com/mraming) - For logging properties that throw exceptions.
- [optical](https://github.com/optical) - For a huge VS 2017 upgrade PR.
- [Jérémie Bertrand](https://github.com/laedit) - For making Serilog.Exceptions compatible with Mono.
- [krajek](https://github.com/krajek) - For writing some much needed unit tests.
