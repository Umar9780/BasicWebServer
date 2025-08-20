using System.Collections.Generic;

public class ApiEndpoint
{
    public string Path { get; set; } = "";
    public string Method { get; set; } = "";
    public string Description { get; set; } = "";
    public string RequestBody { get; set; } = "";
    public string ResponseExample { get; set; } = "";
    public Dictionary<string, string> Parameters { get; set; } = new();
}

public class ApiDocumentation
{
    public string Title { get; set; } = "Basic Web Server API";
    public string Version { get; set; } = "1.0.0";
    public string Description { get; set; } = "A simple web server built from scratch";
    public List<ApiEndpoint> Endpoints { get; set; } = new();

    public void AddEndpoint(string path, string method, string description = "", string requestBody = "", string responseExample = "")
    {
        Endpoints.Add(new ApiEndpoint 
        { 
            Path = path, 
            Method = method, 
            Description = description,
            RequestBody = requestBody,
            ResponseExample = responseExample
        });
    }
}