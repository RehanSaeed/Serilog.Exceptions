namespace Serilog.Exceptions.Test.Destructurers;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using global::Refit;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Xunit;
using static LogJsonOutputUtils;

public class ApiExceptionDestructurerTest
{
    private static Uri requestUri = new("https://foobar.com");

    [Fact]
    public async Task ApiException_HttpStatusCodeIsLoggedAsPropertyAsync()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await BuildException();

        Test_LoggedExceptionContainsProperty(exception, nameof(ApiException.StatusCode), nameof(HttpStatusCode.InternalServerError), options);
    }

    [Fact]
    public async Task ApiException_UriIsLoggedAsPropertyAsync()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await BuildException();
        Test_LoggedExceptionContainsProperty(exception, nameof(ApiException.Uri), requestUri.ToString(), options);
    }

    [Fact]
    public async Task ApiException_ByDefaultContentIsNotLoggedAsPropertyAsync()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await BuildException("hello");
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(ApiException.Content), options);
    }

    [Fact]
    public async Task ApiException_WhenSpecifiedContentIsLoggedAsPropertyAsync()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer(destructureHttpContent: true)]);
        var exception = await BuildException("hello");

        Test_LoggedExceptionContainsProperty(exception, nameof(ApiException.Content), "\"hello\"", options);
    }

    [Fact]
    public async Task ApiException_ByDefaultCommonPropertiesLoggedAsPropertiesAsync()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await BuildException();

        // No need to test all properties, just a handful is sufficient
        Test_LoggedExceptionContainsProperty(exception, nameof(Exception.StackTrace), exception.StackTrace, options);
        Test_LoggedExceptionContainsProperty(exception, nameof(Exception.Message), exception.Message, options);
        Test_LoggedExceptionContainsProperty(exception, nameof(Type), exception.GetType().ToString(), options);
    }

    [Fact]
    public async Task ApiException_WhenSpecifiedCommonPropertiesNotLoggedAsPropertiesAsync()
    {
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer(destructureCommonExceptionProperties: false)]);
        var exception = await BuildException();

        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Exception.StackTrace), options);
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Exception.Message), options);
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Exception.InnerException), options);
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Exception.HelpLink), options);
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Exception.Data), options);
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Exception.HResult), options);
        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(Type), options);
    }

    private static async Task<ApiException> BuildException(string? content = null)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        if (content != null)
        {
            response.Content = JsonContent.Create(content);
        }

        return await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());
    }
}
