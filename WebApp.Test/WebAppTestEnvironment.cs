using System;
using Microsoft.Extensions.DependencyInjection;

namespace WebApp.Test;

public class WebAppTestEnvironment : IDisposable
{
    public WebAppTestEnvironment()
    {
        WebAppHost = new WebAppTestHost();
    }

    public WebAppTestHost WebAppHost { get; }

    public void Dispose()
    {
        WebAppHost?.Dispose();
    }

    public void Start()
    {
        WebAppHost.Start();
    }

    public void Prepare()
    {
        WebAppHost.Services.GetRequiredService<IAccountCache>().Clear();
        WebAppHost.Services.GetRequiredService<IAccountDatabase>().ResetAsync().GetAwaiter().GetResult();
    }
}