using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;

namespace SampleFunctionApp.Tests.Fakes;

public class FakeHttpRequestData(FunctionContext functionContext, Uri url, Stream? body = null)
    : HttpRequestData(functionContext)
{
    private string _method = "GET";
    
    public override Stream Body { get; } = body ?? new MemoryStream();
    public override HttpHeadersCollection Headers { get; } = [];
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = [];
    public override Uri Url { get; } = url;
    public override IEnumerable<ClaimsIdentity> Identities => [];
    public override string Method => _method;

    public void SetMethod(string method)
    {
        _method = method;
    }

    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(FunctionContext);
    }
}