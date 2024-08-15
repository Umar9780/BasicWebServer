
using System.Text.Json;

string prefix = "http://localhost:8080/";

var server = new SimpleWebServer(prefix);

// Add routes
server.AddRoute("/api/hello", "GET", async request =>
{
    var responseObj = new { message = "Hello, World!" };
    return JsonSerializer.Serialize(responseObj);
});

server.AddRoute("/api/data", "POST", async request =>
{
    string data = await HttpUtils.ReadRequestBodyAsync(request);
    var requestData = JsonSerializer.Deserialize<Dictionary<string, string>>(data);
    var responseObj = new { receivedData = requestData };
    return JsonSerializer.Serialize(responseObj);
});

server.AddRoute("/api/update", "PUT", async request =>
{
    string data = await HttpUtils.ReadRequestBodyAsync(request);
    var responseObj = new { message = "Data updated", data };
    return JsonSerializer.Serialize(responseObj);
});

server.AddRoute("/api/delete", "DELETE", async request =>
{
    var responseObj = new { message = "Resource deleted" };
    return JsonSerializer.Serialize(responseObj);
});

await server.StartAsync();
