using VerifyTests.Http;

namespace AvaloniaXKCD.Tests.Orchestration;

public class RecordingHttpClientHandler : DelegatingHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public HttpResponseMessage? LastResponse { get; private set; }

    public RecordingHttpClientHandler(bool record = true)
    {
        InnerHandler = new RecordingHandler(record)
        {
            InnerHandler = new HttpClientHandler()
        };
    }

    public void Stop() => (InnerHandler as RecordingHandler)?.Pause();
    public void Start() => (InnerHandler as RecordingHandler)?.Resume();

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