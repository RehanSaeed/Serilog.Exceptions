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
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionContainsProperty(apiException, nameof(ApiException.StatusCode), nameof(HttpStatusCode.InternalServerError), options);
    }

    [Fact]
    public async Task ApiException_UriIsLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionContainsProperty(apiException, nameof(ApiException.Uri), requestUri.ToString(), options);
    }

    [Fact]
    public async Task ApiException_ByDefaultContentIsNotLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        response.Content = JsonContent.Create("hello");

        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(ApiException.Content), options);
    }

    [Fact]
    public async Task ApiException_WhenSpecifiedContentIsLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer(destructureHttpContent: true)]);
        response.Content = JsonContent.Create("hello");

        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionContainsProperty(apiException, nameof(ApiException.Content), "\"hello\"", options);
    }

    [Fact]
    public async Task ApiException_ByDefaultCommonPropertiesLoggedAsPropertiesAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer()]);
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        // No need to test all properties, just a handful is sufficient
        Test_LoggedExceptionContainsProperty(apiException, nameof(Exception.StackTrace), apiException.StackTrace, options);
        Test_LoggedExceptionContainsProperty(apiException, nameof(Exception.Message), apiException.Message, options);
        Test_LoggedExceptionContainsProperty(apiException, nameof(Type), apiException.GetType().ToString(), options);
    }

    [Fact]
    public async Task ApiException_WhenSpecifiedCommonPropertiesNotLoggedAsPropertiesAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var options = new DestructuringOptionsBuilder().WithDestructurers([new ApiExceptionDestructurer(destructureCommonExceptionProperties: false)]);
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings());

        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Exception.StackTrace), options);
        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Exception.Message), options);
        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Exception.InnerException), options);
        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Exception.HelpLink), options);
        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Exception.Data), options);
        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Exception.HResult), options);
        Test_LoggedExceptionDoesNotContainProperty(apiException, nameof(Type), options);
    }
}
