using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public static class HttpUtils
{
    public static async Task<string> ReadRequestBodyAsync(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return string.Empty;
        }

        using (var body = request.InputStream)
        using (var reader = new StreamReader(body, request.ContentEncoding))
        {
            return await reader.ReadToEndAsync();
        }
    }

    public static async Task<T> ReadRequestBodyAsAsync<T>(HttpListenerRequest request)
    {
        string body = await ReadRequestBodyAsync(request);
        return JsonSerializer.Deserialize<T>(body);
    }
}
