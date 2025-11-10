using VerifyTests.Http;

namespace AvaloniaXKCD.Tests.Orchestration;

public class MockHttpClientHandler : DelegatingHandler
{
    private readonly Dictionary<string, HttpResponseMessage> _mockedResponses = new();

    public HttpRequestMessage? LastRequest { get; private set; }
    public HttpResponseMessage? LastResponse { get; private set; }

    public MockHttpClientHandler()
    {
        InnerHandler = new MockHttpHandler((request) =>
        {
            var requestKey = $"{request.RequestUri?.AbsolutePath}_{request.Method}";
            if (_mockedResponses.TryGetValue(requestKey, out var response))
            {
                return response;
            }

            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        });
    }

    public void AddMockedResponse(HttpRequestMessage request, HttpResponseMessage response)
    {
        var requestKey = $"{new Uri(new Uri("http://localhost"), request.RequestUri!).AbsolutePath}_{request.Method}";
        _mockedResponses[requestKey] = response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancel)
    {
        LastRequest = request;
        var response = await base.SendAsync(request, cancel);
        LastResponse = response;
        return response;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancel)
    {
        LastRequest = request;
        var response = base.Send(request, cancel);
        LastResponse = response;
        return response;
    }
}