using System.Net;
using System.Text;
using System.Threading.Tasks;

public class RequestHandler
{
    private readonly RouteManager _routeManager;

    public RequestHandler(RouteManager routeManager)
    {
        _routeManager = routeManager;
    }

    public async Task HandleRequestAsync(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        string responseString;
        string contentType;

        if (request.Url.AbsolutePath == "/")
        {
            responseString = await _routeManager.HandleRootRequest(request);
            contentType = "text/html";
        }
        else
        {
            responseString = await _routeManager.HandleRequestAsync(request);
            contentType = request.HttpMethod == "POST" || request.HttpMethod == "PUT" ? "application/json" : "text/html";
        }

        response.ContentType = contentType;
        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        response.Close();
    }
}
