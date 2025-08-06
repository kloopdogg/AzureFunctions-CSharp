using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace SampleFunctionApp.Functions;

public class SampleHttpFunctions(ILogger<SampleHttpFunctions> _logger)
{
    [Function("WelcomeMessage")]
    public async Task<HttpResponseData> WelcomeMessage(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData request)
    {
        _logger.LogInformation("WelcomeMessage function processing a {Method} request for url: {Url}", request.Method, request.Url.AbsoluteUri);

        string name = request.Query["name"] ?? await request.ReadAsStringAsync() ?? string.Empty;
        string messagePrefix = !string.IsNullOrEmpty(name) ? $"{name}, " : "";

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"{messagePrefix}Azure Functions <⚡> are awesome!");
        return response;
    }
}