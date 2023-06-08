using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ArcadeBot.NetTests.WebSockets.TestServer;

internal class TestServerApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override Microsoft.AspNetCore.TestHost.TestServer CreateServer(IWebHostBuilder builder) =>
        base.CreateServer(
            builder.UseSolutionRelativeContentRoot(""));

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseSerilog();
        return base.CreateHost(builder);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseStartup<TStartup>();
    }
    protected override IWebHostBuilder CreateWebHostBuilder() =>
        WebHost.CreateDefaultBuilder();



}
