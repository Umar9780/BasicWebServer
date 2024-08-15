using System;
using System.Net;
using System.Threading.Tasks;

public class SimpleWebServer
{
    private readonly HttpListener _listener;
    private readonly RouteManager _routeManager;
    private readonly RequestHandler _requestHandler;

    public SimpleWebServer(string prefix)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(prefix);
        _routeManager = new RouteManager();
        _requestHandler = new RequestHandler(_routeManager);
    }

    public void AddRoute(string path, string method, Func<HttpListenerRequest, Task<string>> handler)
    {
        _routeManager.AddRoute(path, method, handler);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"Listening for requests on {_listener.Prefixes.FirstOrDefault().ToString()}");

        while (true)
        {
            // Wait for incoming request asynchronously
            HttpListenerContext context = await _listener.GetContextAsync();
            await _requestHandler.HandleRequestAsync(context);
        }
    }

    public void Stop()
    {
        _listener.Stop();
        _listener.Close();
    }
}
