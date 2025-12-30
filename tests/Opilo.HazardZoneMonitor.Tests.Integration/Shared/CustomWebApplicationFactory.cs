﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Opilo.HazardZoneMonitor.Api;

namespace Opilo.HazardZoneMonitor.Tests.Integration.Shared;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Development");
    }
}
