using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace SampleFunctionApp.Tests.Mocks;

public static class MockResponseDataHelper
{
    internal static T GetBodyObjectFromResponse<T>(HttpResponseData response)
    {
        string body = GetBodyStringFromResponse(response);

        T? result = JsonSerializer.Deserialize<T>(body);
        return result == null ? throw new InvalidDataException("Deserialized returned null") : result;
    }

    internal static string GetBodyStringFromResponse(HttpResponseData response)
    {
        string body = string.Empty;
        using (StreamReader reader = new(response.Body))
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            body = reader.ReadToEnd();
        }
        return body;
    }
}