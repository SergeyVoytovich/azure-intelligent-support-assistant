using System.Net;
using System.Text;

namespace SupportAssistant.UnitTests.TestDoubles;

// No request can leave the process. Unexpected calls fail instead of reaching Azure.
internal sealed class StubHttpHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode Status, string Body)> responses = new();
    public List<(HttpMethod Method, Uri? Uri, string Body)> Requests { get; } = [];

    public void Enqueue(string body, HttpStatusCode status = HttpStatusCode.OK)
        => responses.Enqueue((status, body));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Requests.Add((request.Method, request.RequestUri,
            request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken)));
        Assert.NotEmpty(responses);
        var response = responses.Dequeue();
        return new HttpResponseMessage(response.Status)
        {
            Content = new StringContent(response.Body, Encoding.UTF8, "application/json"),
            RequestMessage = request
        };
    }
}
