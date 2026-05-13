using System.Net;

namespace RobotManagementSystem.Tests.Helpers;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _httpResponse;
    private readonly HttpStatusCode _statusCode;
    public HttpRequestMessage? LastSentRequest { get; set; }
    
    public MockHttpMessageHandler(string httpResponse, HttpStatusCode statusCode)
    {
        _httpResponse = httpResponse;
        _statusCode = statusCode;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastSentRequest = request;
        
        // in tests: Assert.Equal(HttpMethod.Get, request.Method);
        // Assert.Equal("api/map", handler.LastRequest.RequestUri!.AbsolutePath);
        
        return Task.FromResult(new HttpResponseMessage(_statusCode) {Content = new StringContent(_httpResponse)});
    }
}