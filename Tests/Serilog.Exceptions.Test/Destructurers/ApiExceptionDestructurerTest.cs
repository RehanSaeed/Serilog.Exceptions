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
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionContainsProperty(exception, nameof(ApiException.StatusCode), nameof(HttpStatusCode.InternalServerError), options);
    }

    [Fact]
    public async Task ApiException_UriIsLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionContainsProperty(exception, nameof(ApiException.Uri), requestUri.ToString(), options);
    }

    [Fact]
    public async Task ApiException_ByDefaultContentIsNotLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        response.Content = JsonContent.Create("hello");

        var exception = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionDoesNotContainProperty(exception, nameof(ApiException.Content), options);
    }

    [Fact]
    public async Task ApiException_WhenSpecifiedContentIsLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer(destructureHttpContent: true)]);
        response.Content = JsonContent.Create("hello");

        var exception = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionContainsProperty(exception, nameof(ApiException.Content), "\"hello\"", options);
    }

    [Fact]
    public async Task ApiException_ByDefaultCommonPropertiesLoggedAsPropertiesAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var exception = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

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

    private static async Task<ApiException> BuildException()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        return await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());
    }
}
