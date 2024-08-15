using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class RouteManager
{
    private readonly Dictionary<string, Func<HttpListenerRequest, Task<string>>> _routes;
    private readonly Dictionary<string, string> _routeMethods;

    public RouteManager()
    {
        _routes = new Dictionary<string, Func<HttpListenerRequest, Task<string>>>();
        _routeMethods = new Dictionary<string, string>();
    }

    public void AddRoute<T>(string path, string method, Func<T, Task<string>> handler)
    {
        _routes[path] = async request =>
        {
            T data = await HttpUtils.ReadRequestBodyAsAsync<T>(request);
            return await handler(data);
        };
        _routeMethods[path] = method;
    }

    public Task<string> HandleRequestAsync(HttpListenerRequest request)
    {
        if (_routes.TryGetValue(request.Url.AbsolutePath, out var handler))
        {
            // Ensure that the request method matches the route method
            if (_routeMethods.TryGetValue(request.Url.AbsolutePath, out var method) &&
                request.HttpMethod != method)
            {
                return Task.FromResult("405 Method Not Allowed");
            }

            return handler(request);
        }
        else
        {
            return Task.FromResult("404 Not Found");
        }
    }

    public Task<string> HandleRootRequest(HttpListenerRequest request)
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Simple Web Server</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <h1>Simple Web Server</h1>
    <p>Welcome to the Simple Web Server. Below is a list of available routes and their HTTP methods:</p>
    <table>
        <thead>
            <tr>
                <th>Route</th>
                <th>HTTP Method</th>
            </tr>
        </thead>
        <tbody>";

        foreach (var route in _routeMethods)
        {
            html += $@"
            <tr>
                <td>{route.Key}</td>
                <td>{route.Value}</td>
            </tr>";
        }

        html += @"
        </tbody>
    </table>
</body>
</html>";

        return Task.FromResult(html);
    }
}
