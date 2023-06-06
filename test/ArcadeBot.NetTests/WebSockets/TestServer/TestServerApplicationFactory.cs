using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace ArcadeBot.NetTests.WebSockets.TestServer;

internal class TestServerApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override Microsoft.AspNetCore.TestHost.TestServer CreateServer(IWebHostBuilder builder) =>
        base.CreateServer(
            builder.UseSolutionRelativeContentRoot(""));

    protected override IWebHostBuilder CreateWebHostBuilder() =>
        WebHost.CreateDefaultBuilder()
            .UseStartup<TStartup>();



}
