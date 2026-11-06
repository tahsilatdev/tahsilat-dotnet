using System;
using System.Net.Http;
using System.Text;
using Tahsilat.NET.Infrastructure.Http;

namespace Tahsilat.NET.Services.Implementations
{
    internal abstract class BaseService
    {
        protected readonly ITahsilatHttpClient _http;

        protected BaseService(ITahsilatHttpClient httpClient)
        {
            _http = httpClient;
        }

        protected HttpRequestMessage CreatePost(string endpoint, object body)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = FormUrlEncodedContentBuilder.Build(body)
            };

            return request;
        }

        protected HttpRequestMessage CreateGet(string endpoint)
        {
            return new HttpRequestMessage(HttpMethod.Get, endpoint);
        }

        /// <summary>
        /// GET with the request object's properties sent as query string parameters.
        /// A GET body is ignored by the API and rejected outright by .NET Framework's
        /// HttpClient, so filters must travel in the URL.
        /// </summary>
        protected HttpRequestMessage CreateGet(string endpoint, object? query)
        {
            return new HttpRequestMessage(HttpMethod.Get, endpoint + QueryStringBuilder.Build(query));
        }

        protected HttpRequestMessage CreateDelete(string endpoint)
        {
            // The API requires a Content-Type header on every non-GET request
            // and answers 415 without one. Send an empty JSON body, the same
            // way the token request does.
            return new HttpRequestMessage(HttpMethod.Delete, endpoint)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
            };
        }
    }
}
