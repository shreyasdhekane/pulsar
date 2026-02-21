using Microsoft.AspNetCore.SignalR;

namespace Pulsar.API.Hubs;

public class PulsarHub : Hub
{
    public async Task JoinEndpointGroup(string endpointId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"endpoint-{endpointId}");
    }
}