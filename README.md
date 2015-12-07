<h1>
    <img src="https://raw.githubusercontent.com/RehanSaeed/Serilog.Exceptions/master/Images/Serilog%20Community%20256x256.png" alt="Serilog.Exceptions Logo" width="30px" height="30px" /> <a href="https://github.com/RehanSaeed/ASP.NET-MVC-Boilerplate">Serilog.Exceptions</a>
</h1>
Log exception details and custom properties that are not output in Exception.ToString().

## Getting Started

Add the Serilog.Exceptions NuGet package to your project using the NuGet Package Manager or run the following command in the Package Console Window:

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
