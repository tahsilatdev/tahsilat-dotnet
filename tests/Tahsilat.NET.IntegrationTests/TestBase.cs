using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tahsilat.NET.Abstractions;
using Tahsilat.NET.Configuration;

namespace Tahsilat.NET.IntegrationTests
{
    public abstract class TestBase
    {
        protected readonly ITahsilatClient Client;
        protected readonly string WebhookSecret;

        public TestBase()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.local.json", optional: true)
                .Build();

            var apiKey = config["Tahsilat:ApiKey"];
            WebhookSecret = config["Tahsilat:WebhookSecret"] ?? "";

            Client = new TahsilatClient(new TahsilatClientOptions
            {
                ApiKey = apiKey,
                TimeoutSeconds = 500
            });
            
        }
    }
}
