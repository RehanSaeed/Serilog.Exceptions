namespace Serilog.Exceptions.Test.Destructurers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using global::Refit;
    using Xunit;
    using static LogJsonOutputUtils;

    public class ApiExceptionDestructurerTest
    {
        [Fact]
        public async Task ApiException_HttpStatusCodeIsLoggedAsPropertyAsync()
        {
            using var message = new HttpRequestMessage(HttpMethod.Get, new Uri("https://foobar.com"));
            using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);
            Test_LoggedExceptionContainsProperty(apiException, nameof(ApiException.StatusCode), nameof(HttpStatusCode.InternalServerError));
        }

        [Fact]
        public async Task ApiException_UriIsLoggedAsPropertyAsync()
        {
            var requestUri = new Uri("https://foobar.com");
            using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);

            Test_LoggedExceptionContainsProperty(apiException, nameof(ApiException.Uri), requestUri.ToString());
        }

        [Fact]
        public async Task ApiException_ContentIsLoggedAsPropertyAsync()
        {
            var requestUri = new Uri("https://foobar.com");
            using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            response.Content = JsonContent.Create("hello");

            var apiException = await ApiException.Create(message, HttpMethod.Get, response, new RefitSettings()).ConfigureAwait(false);

            Test_LoggedExceptionContainsProperty(apiException, nameof(ApiException.Content), "\"hello\"");
        }
    }
}
