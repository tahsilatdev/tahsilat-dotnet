using System;
using System.Net.Http;
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

        protected HttpRequestMessage CreateGet(string endpoint, object body)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint)
            {
                Content = FormUrlEncodedContentBuilder.Build(body)
            };
            return request;
        }

        protected HttpRequestMessage CreateDelete(string endpoint)
        {
            return new HttpRequestMessage(HttpMethod.Delete, endpoint);
        }
    }
}
