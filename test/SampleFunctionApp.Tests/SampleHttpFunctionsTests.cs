using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;
using SampleFunctionApp.Functions;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using SampleFunctionApp.Tests.Fakes;
using Microsoft.Azure.Functions.Worker;
using Azure.Core.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using SampleFunctionApp.Tests.Mocks;
using System.Text;

namespace SampleFunctionApp.Tests;

[TestClass]
public sealed class SampleHttpFunctionsTests
{
    private Mock<ILogger<SampleHttpFunctions>> _mockLogger = null!;
    private Mock<FunctionContext> _mockFunctionContext = null!;
    private FakeHttpRequestData _fakeRequestData = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<SampleHttpFunctions>>();

        WorkerOptions workerOptions = new() { Serializer = new JsonObjectSerializer() };
        IOptions<WorkerOptions> options = Options.Create(workerOptions);
        ServiceCollection services = new();
        services.AddSingleton(options);
        _mockFunctionContext = new Mock<FunctionContext>();
        _mockFunctionContext.Setup(c => c.InstanceServices).Returns(services.BuildServiceProvider());

        _fakeRequestData = new FakeHttpRequestData(
            _mockFunctionContext.Object,
            new Uri("http://localhost/api/WelcomeMessage")
        );
    }

    private SampleHttpFunctions CreateFunctions()
    {
        SampleHttpFunctions functions = new(_mockLogger.Object);
        return functions;
    }

    private FakeHttpRequestData CreateFakeRequestData(string url, string method = "GET", string mediaType = "application/json", string body = "")
    {
        Stream contentStream = Stream.Null;
        if (!string.IsNullOrEmpty(body))
        {
            StringContent content = new(body, Encoding.UTF8, mediaType);
            contentStream = content.ReadAsStream();
        }

        FakeHttpRequestData requestData = new(
            _mockFunctionContext.Object,
            new Uri(url),
            contentStream == Stream.Null ? null : contentStream);
        requestData.SetMethod(method);
        return requestData;
    }

    private void VerifyLog(LogLevel logLevel, string logMessage)
    {
        _mockLogger.Verify(
            x => x.Log<It.IsAnyType>(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == logMessage),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [TestMethod]
    public async Task Get_WelcomeMessage_NoQueryString_LogsProcessingMessage()
    {
        // Arrange
        string expectedLogMessage = $"WelcomeMessage function processing a {_fakeRequestData.Method} request for url: {_fakeRequestData.Url.AbsoluteUri}";

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(_fakeRequestData);

        // Assert
        VerifyLog(LogLevel.Information, expectedLogMessage);
    }

    [TestMethod]
    public async Task Get_WelcomeMessage_NameInQueryString_LogsProcessingMessage()
    {
        // Arrange
        string name = "Jimmy";
        string url = $"http://localhost/api/WelcomeMessage?name={name}";
        FakeHttpRequestData requestWithNameQueryString = new(
            _mockFunctionContext.Object,
            new Uri(url)
        );
        string expectedLogMessage = $"WelcomeMessage function processing a {_fakeRequestData.Method} request for url: {url}";

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithNameQueryString);

        // Assert
        VerifyLog(LogLevel.Information, expectedLogMessage);
    }

    [TestMethod]
    public async Task Get_WelcomeMessage_NoQueryString_ReturnsOk()
    {
        // Arrange
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
        string expectedMessage = "Azure Functions <⚡> are awesome!";

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(_fakeRequestData);

        // Assert
        Assert.AreEqual(expectedStatusCode, response.StatusCode);

        string result = MockResponseDataHelper.GetBodyStringFromResponse(response);
        Assert.AreEqual(expectedMessage, result);
    }

    [TestMethod]
    public async Task Get_WelcomeMessage_NameInQueryString_ReturnsOk()
    {
        // Arrange
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
        string name = "Jimmy";
        string expectedMessage = $"{name}, Azure Functions <⚡> are awesome!";
        string url = $"http://localhost/api/WelcomeMessage?name={name}";
        FakeHttpRequestData requestWithNameQueryString = new(
            _mockFunctionContext.Object,
            new Uri(url)
        );

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithNameQueryString);

        // Assert
        Assert.AreEqual(expectedStatusCode, response.StatusCode);

        string result = MockResponseDataHelper.GetBodyStringFromResponse(response);
        Assert.AreEqual(expectedMessage, result);
    }

    [TestMethod]
    public async Task Post_WelcomeMessage_NoQueryString_LogsProcessingMessage()
    {
        // Arrange
        FakeHttpRequestData requestWithNameQueryString = CreateFakeRequestData(
            "http://localhost/api/WelcomeMessage",
            "POST",
            "text/plain",
            string.Empty
        );
        string expectedLogMessage = $"WelcomeMessage function processing a {requestWithNameQueryString.Method} request for url: {requestWithNameQueryString.Url.AbsoluteUri}";

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithNameQueryString);

        // Assert
        VerifyLog(LogLevel.Information, expectedLogMessage);
    }

    [TestMethod]
    public async Task Post_WelcomeMessage_NameInQueryString_LogsProcessingMessage()
    {
        // Arrange
        string name = "Jimmy";
        string url = $"http://localhost/api/WelcomeMessage?name={name}";
        FakeHttpRequestData requestWithBody = CreateFakeRequestData(
            url,
            "POST",
            "text/plain",
            string.Empty
        );
        string expectedLogMessage = $"WelcomeMessage function processing a {requestWithBody.Method} request for url: {url}";

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithBody);

        // Assert
        VerifyLog(LogLevel.Information, expectedLogMessage);
    }

    [TestMethod]
    public async Task Post_WelcomeMessage_NoName_ReturnsOk()
    {
        // Arrange
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
        string expectedMessage = "Azure Functions <⚡> are awesome!";
        FakeHttpRequestData requestWithBody = CreateFakeRequestData(
            "http://localhost/api/WelcomeMessage",
            "POST",
            "text/plain",
            string.Empty
        );

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithBody);

        // Assert
        Assert.AreEqual(expectedStatusCode, response.StatusCode);

        string result = MockResponseDataHelper.GetBodyStringFromResponse(response);
        Assert.AreEqual(expectedMessage, result);
    }

    [TestMethod]
    public async Task Post_WelcomeMessage_NameInQueryString_ReturnsOk()
    {
        // Arrange
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
        string name = "Jimmy";
        string url = $"http://localhost/api/WelcomeMessage?name={name}";
        string expectedMessage = $"{name}, Azure Functions <⚡> are awesome!";
        FakeHttpRequestData requestWithNameQueryString = CreateFakeRequestData(
            url,
            "POST",
            "text/plain",
            string.Empty
        );

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithNameQueryString);

        // Assert
        Assert.AreEqual(expectedStatusCode, response.StatusCode);

        string result = MockResponseDataHelper.GetBodyStringFromResponse(response);
        Assert.AreEqual(expectedMessage, result);
    }

    [TestMethod]
    public async Task Post_WelcomeMessage_NameInBody_ReturnsOk()
    {
        // Arrange
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
        string name = "Jimmy";
        string expectedMessage = $"{name}, Azure Functions <⚡> are awesome!";
        FakeHttpRequestData requestWithBody = CreateFakeRequestData(
            "http://localhost/api/WelcomeMessage",
            "POST",
            "text/plain",
            name
        );

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithBody);

        // Assert
        Assert.AreEqual(expectedStatusCode, response.StatusCode);

        string result = MockResponseDataHelper.GetBodyStringFromResponse(response);
        Assert.AreEqual(expectedMessage, result);
    }

    [TestMethod]
    public async Task Post_WelcomeMessage_NameInBoth_ReturnsOk()
    {
        // Arrange
        HttpStatusCode expectedStatusCode = HttpStatusCode.OK;
        string name = "Jimmy";
        string bodyName = "John";
        string expectedMessage = $"{name}, Azure Functions <⚡> are awesome!";
        FakeHttpRequestData requestWithBody = CreateFakeRequestData(
            $"http://localhost/api/WelcomeMessage?name={name}",
            "POST",
            "text/plain",
            bodyName
        );

        // Act
        SampleHttpFunctions functions = CreateFunctions();
        var response = await functions.WelcomeMessage(requestWithBody);

        // Assert
        Assert.AreEqual(expectedStatusCode, response.StatusCode);

        string result = MockResponseDataHelper.GetBodyStringFromResponse(response);
        Assert.AreEqual(expectedMessage, result);
    }
}