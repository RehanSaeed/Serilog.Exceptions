namespace Serilog.Exceptions.Test.Destructurers;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using global::Refit;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;
using Xunit;
using static LogJsonOutputUtils;

public class ValidationApiExceptionDestructurerTests
{
    private const string ProblemDetailsJson = "application/problem+json";
    private readonly ProblemDetails problemDetailsBadRequest;

    public ValidationApiExceptionDestructurerTests() => this.problemDetailsBadRequest = new ProblemDetails
    {
        Detail = "A validation failure has occurred",
        Status = (int)HttpStatusCode.BadRequest,
        Title = "Bad Request",
        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    };

    [Fact]
    public async Task ValidationApiException_HttpStatusCodeIsLoggedAsPropertyAsync()
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, new Uri("https://foobar.com"));
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var responseContent = JsonSerializer.Serialize(this.problemDetailsBadRequest);
        response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, ProblemDetailsJson);
        var options = new DestructuringOptionsBuilder().WithDestructurers(new[] { new ValidationApiExceptionDestructurer() });
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
        var validationApiException = ValidationApiException.Create(apiException);

        Test_LoggedExceptionContainsProperty(validationApiException, nameof(ValidationApiException.StatusCode), nameof(HttpStatusCode.BadRequest), options);
    }

    [Fact]
    public async Task ValidationApiException_UriIsLoggedAsPropertyAsync()
    {
        var requestUri = new Uri("https://foobar.com");
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var responseContent = JsonSerializer.Serialize(this.problemDetailsBadRequest);
        response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, ProblemDetailsJson);
        var options = new DestructuringOptionsBuilder().WithDestructurers(new[] { new ValidationApiExceptionDestructurer() });
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
        var validationApiException = ValidationApiException.Create(apiException);

        Test_LoggedExceptionContainsProperty(validationApiException, nameof(ValidationApiException.Uri), requestUri.ToString(), options);
    }

    [Fact]
    public async Task ValidationApiException_ByDefaultContentIsNotLoggedAsPropertyAsync()
    {
        var requestUri = new Uri("https://foobar.com");
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var responseContent = JsonSerializer.Serialize(this.problemDetailsBadRequest);
        response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, ProblemDetailsJson);
        var options = new DestructuringOptionsBuilder().WithDestructurers(new[] { new ValidationApiExceptionDestructurer() });
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
        var validationApiException = ValidationApiException.Create(apiException);

        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(ValidationApiException.Content), options);
    }

    [Fact]
    public async Task ValidationApiException_WhenSpecifiedContentIsLoggedAsPropertyAsync()
    {
        var requestUri = new Uri("https://foobar.com");
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var responseContent = JsonSerializer.Serialize(this.problemDetailsBadRequest);
        response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, ProblemDetailsJson);
        var options = new DestructuringOptionsBuilder().WithDestructurers(new[] { new ValidationApiExceptionDestructurer(destructureHttpContent: true) });
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
        var validationApiException = ValidationApiException.Create(apiException);

        Test_LoggedExceptionContainsProperty(
            validationApiException,
            nameof(ValidationApiException.Content),
            nameof(this.problemDetailsBadRequest.Status),
            this.problemDetailsBadRequest.Status.ToString(System.Globalization.CultureInfo.InvariantCulture),
            options);
        Test_LoggedExceptionContainsProperty(
            validationApiException,
            nameof(ValidationApiException.Content),
            nameof(this.problemDetailsBadRequest.Title),
            this.problemDetailsBadRequest.Title,
            options);
        Test_LoggedExceptionContainsProperty(
            validationApiException,
            nameof(ValidationApiException.Content),
            nameof(this.problemDetailsBadRequest.Detail),
            this.problemDetailsBadRequest.Detail,
            options);
    }

    [Fact]
    public async Task ValidationApiException_ByDefaultCommonPropertiesLoggedAsPropertiesAsync()
    {
        var requestUri = new Uri("https://foobar.com");
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var responseContent = JsonSerializer.Serialize(this.problemDetailsBadRequest);
        response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, ProblemDetailsJson);
        var options = new DestructuringOptionsBuilder().WithDestructurers(new[] { new ValidationApiExceptionDestructurer() });
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
        var validationApiException = ValidationApiException.Create(apiException);

        // No need to test all properties, just a handful is sufficient
        Test_LoggedExceptionContainsProperty(validationApiException, nameof(Exception.StackTrace), validationApiException.StackTrace, options);
        Test_LoggedExceptionContainsProperty(validationApiException, nameof(Exception.Message), validationApiException.Message, options);
        Test_LoggedExceptionContainsProperty(validationApiException, nameof(Type), apiException.GetType().ToString(), options);
    }

    [Fact]
    public async Task ValidationApiException_WhenSpecifiedCommonPropertiesNotLoggedAsPropertiesAsync()
    {
        var requestUri = new Uri("https://foobar.com");
        using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
        using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var responseContent = JsonSerializer.Serialize(this.problemDetailsBadRequest);
        response.Content = new StringContent(responseContent, System.Text.Encoding.UTF8, ProblemDetailsJson);
        var options = new DestructuringOptionsBuilder().WithDestructurers(new[] { new ValidationApiExceptionDestructurer(destructureCommonExceptionProperties: false) });
        var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
        var validationApiException = ValidationApiException.Create(apiException);

        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Exception.StackTrace), options);
        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Exception.Message), options);
        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Exception.InnerException), options);
        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Exception.HelpLink), options);
        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Exception.Data), options);
        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Exception.HResult), options);
        Test_LoggedExceptionDoesNotContainProperty(validationApiException, nameof(Type), options);
    }
}
