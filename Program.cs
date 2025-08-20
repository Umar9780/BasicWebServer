
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

string prefix = "http://localhost:8080/";

var server = new SimpleWebServer(prefix);

// Add routes with documentation
server.AddRoute("/api/hello", "GET", async request =>
{
    var responseObj = new { message = "Hello, World!" };
    return JsonSerializer.Serialize(responseObj);
}, 
"Returns a simple greeting message", 
"", 
JsonSerializer.Serialize(new { message = "Hello, World!" }));

server.AddRoute("/api/data", "POST", async request =>
{
    string data = await HttpUtils.ReadRequestBodyAsync(request);
    var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
    var responseObj = new { receivedData = requestData };
    return JsonSerializer.Serialize(responseObj);
}, 
"Accepts JSON data and returns it back", 
JsonSerializer.Serialize(new { name = "John", age = "30" }), 
JsonSerializer.Serialize(new { receivedData = new { name = "John", age = "30" } }));

server.AddRoute("/api/update", "PUT", async request =>
{
    string data = await HttpUtils.ReadRequestBodyAsync(request);
    var responseObj = new { message = "Data updated", data };
    return JsonSerializer.Serialize(responseObj);
}, 
"Updates data and returns confirmation", 
JsonSerializer.Serialize(new { id = 1, name = "Updated Name" }), 
JsonSerializer.Serialize(new { message = "Data updated", data = "{\"id\":1,\"name\":\"Updated Name\"}" }));

server.AddRoute("/api/delete", "DELETE", async request =>
{
    var responseObj = new { message = "Resource deleted" };
    return JsonSerializer.Serialize(responseObj);
}, 
"Deletes a resource and returns confirmation", 
"", 
JsonSerializer.Serialize(new { message = "Resource deleted" }));

// Open browser automatically
OpenBrowser(prefix);

await server.StartAsync();

static void OpenBrowser(string url)
{
    try
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        //else if (RuntimeInformation.IsOSPlatform(OSPlatform.Mac))
        //{
        //    Process.Start("open", url);
        //}
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unable to open browser: {ex.Message}");
    }
}
