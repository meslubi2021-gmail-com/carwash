#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using Azure.Identity;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;

namespace CarWash.PWA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    var config = builder.Build();
                    var keyVaultBaseUri = new Uri(config.GetValue<string>("KeyVault:BaseUrl"));

                    builder.AddAzureKeyVault(keyVaultBaseUri, new DefaultAzureCredential());
                })
                .ConfigureKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>();
    }
}
