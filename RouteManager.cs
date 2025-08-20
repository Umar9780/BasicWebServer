using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class RouteManager
{
    private readonly Dictionary<string, Func<HttpListenerRequest, Task<string>>> _routes;
    private readonly Dictionary<string, string> _routeMethods;
    private readonly ApiDocumentation _apiDocumentation;

    public RouteManager()
    {
        _routes = new Dictionary<string, Func<HttpListenerRequest, Task<string>>>();
        _routeMethods = new Dictionary<string, string>();
        _apiDocumentation = new ApiDocumentation();
    }

    public void AddRoute(string path, string method, Func<HttpListenerRequest, Task<string>> handler, string description = "", string requestBody = "", string responseExample = "")
    {
        _routes[path] = handler;
        _routeMethods[path] = method;
        _apiDocumentation.AddEndpoint(path, method, description, requestBody, responseExample);
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
        var html = GenerateSwaggerUI();
        return Task.FromResult(html);
    }

    private string GenerateSwaggerUI()
    {
        var endpointsHtml = "";
        foreach (var endpoint in _apiDocumentation.Endpoints)
        {
            var methodColor = GetMethodColor(endpoint.Method);
            var safeId = endpoint.Path.Replace("/", "_").Replace("-", "_").TrimStart('_');
            endpointsHtml += $@"
                <div class='endpoint' id='{safeId}'>
                    <div class='endpoint-header' onclick='toggleEndpoint(""{safeId}"")'>
                        <span class='method method-{endpoint.Method.ToLower()}' style='background-color: {methodColor};'>{endpoint.Method}</span>
                        <span class='path'>{endpoint.Path}</span>
                        <span class='description'>{endpoint.Description}</span>
                        <span class='toggle'>▼</span>
                    </div>
                    <div class='endpoint-details' id='details_{safeId}'>
                        <div class='section'>
                            <h4>Try it out</h4>
                            <form class='api-form' onsubmit='testEndpoint(event, ""{endpoint.Path}"", ""{endpoint.Method}"", ""{safeId}"")'>
                                {(string.IsNullOrEmpty(endpoint.RequestBody) ? "" : $@"
                                <div class='form-group'>
                                    <label>Request Body (JSON):</label>
                                    <textarea name='requestBody' rows='4' placeholder='{endpoint.RequestBody}'>{endpoint.RequestBody}</textarea>
                                </div>")}
                                <button type='submit' class='execute-btn'>Execute</button>
                            </form>
                        </div>
                        {(!string.IsNullOrEmpty(endpoint.ResponseExample) ? $@"
                        <div class='section'>
                            <h4>Response Example</h4>
                            <pre class='code-block'>{endpoint.ResponseExample}</pre>
                        </div>" : "")}
                        <div class='section'>
                            <h4>Response</h4>
                            <div id='response_{safeId}' class='response-container'></div>
                        </div>
                    </div>
                </div>";
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>{_apiDocumentation.Title} - API Documentation</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #fafafa; }}
        .header {{ background: #1f2937; color: white; padding: 20px; }}
        .header h1 {{ font-size: 24px; margin-bottom: 5px; }}
        .header p {{ opacity: 0.8; }}
        .container {{ max-width: 1200px; margin: 0 auto; padding: 20px; }}
        .endpoint {{ background: white; margin: 10px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .endpoint-header {{ padding: 15px 20px; cursor: pointer; display: flex; align-items: center; border-bottom: 1px solid #e5e7eb; }}
        .endpoint-header:hover {{ background: #f9fafb; }}
        .method {{ padding: 4px 8px; border-radius: 4px; color: white; font-weight: bold; font-size: 12px; min-width: 60px; text-align: center; margin-right: 15px; }}
        .path {{ font-family: 'Courier New', monospace; font-weight: 500; font-size: 16px; margin-right: 15px; }}
        .description {{ color: #6b7280; flex: 1; }}
        .toggle {{ margin-left: auto; transition: transform 0.2s; }}
        .endpoint-details {{ display: none; padding: 20px; }}
        .endpoint-details.active {{ display: block; }}
        .section {{ margin: 20px 0; }}
        .section h4 {{ color: #374151; margin-bottom: 10px; font-size: 14px; font-weight: 600; }}
        .form-group {{ margin: 15px 0; }}
        .form-group label {{ display: block; margin-bottom: 5px; font-weight: 500; color: #374151; }}
        .form-group textarea {{ width: 100%; padding: 10px; border: 1px solid #d1d5db; border-radius: 4px; font-family: 'Courier New', monospace; font-size: 13px; }}
        .execute-btn {{ background: #3b82f6; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; font-weight: 500; }}
        .execute-btn:hover {{ background: #2563eb; }}
        .code-block {{ background: #f3f4f6; padding: 15px; border-radius: 4px; font-family: 'Courier New', monospace; font-size: 13px; overflow-x: auto; }}
        .response-container {{ min-height: 50px; background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 4px; padding: 15px; }}
        .response-success {{ background: #f0fdf4; border-color: #bbf7d0; color: #166534; }}
        .response-error {{ background: #fef2f2; border-color: #fecaca; color: #dc2626; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{_apiDocumentation.Title}</h1>
        <p>Version {_apiDocumentation.Version} - {_apiDocumentation.Description}</p>
    </div>
    <div class='container'>
        {endpointsHtml}
    </div>
    
    <script>
        function toggleEndpoint(id) {{
            const details = document.getElementById('details_' + id);
            const toggle = document.querySelector(`#${{id.replace('_', '')}} .toggle`);
            
            if (details.classList.contains('active')) {{
                details.classList.remove('active');
                toggle.style.transform = 'rotate(0deg)';
            }} else {{
                details.classList.add('active');
                toggle.style.transform = 'rotate(180deg)';
            }}
        }}

        async function testEndpoint(event, path, method, safeId) {{
            event.preventDefault();
            const form = event.target;
            const responseContainer = document.getElementById('response_' + safeId);
            
            if (!responseContainer) {{
                console.error('Response container not found for ID:', 'response_' + safeId);
                return;
            }}
            
            try {{
                responseContainer.innerHTML = '<div style=""color: #6b7280;"">Loading...</div>';
                
                const options = {{
                    method: method,
                    headers: {{
                        'Content-Type': 'application/json'
                    }}
                }};
                
                const requestBody = form.requestBody ? form.requestBody.value : null;
                if (requestBody && (method === 'POST' || method === 'PUT')) {{
                    options.body = requestBody;
                }}
                
                const response = await fetch(path, options);
                const responseText = await response.text();
                
                responseContainer.className = 'response-container ' + (response.ok ? 'response-success' : 'response-error');
                responseContainer.innerHTML = `
                    <div><strong>Status:</strong> ${{response.status}} ${{response.statusText}}</div>
                    <div style=""margin-top: 10px;""><strong>Response:</strong></div>
                    <pre style=""margin-top: 5px; white-space: pre-wrap;"">${{responseText}}</pre>
                `;
            }} catch (error) {{
                responseContainer.className = 'response-container response-error';
                responseContainer.innerHTML = `<div><strong>Error:</strong> ${{error.message}}</div>`;
            }}
        }}
    </script>
</body>
</html>";
    }

    private string GetMethodColor(string method)
    {
        return method.ToUpper() switch
        {
            "GET" => "#10b981",
            "POST" => "#3b82f6", 
            "PUT" => "#f59e0b",
            "DELETE" => "#ef4444",
            _ => "#6b7280"
        };
    }
}
