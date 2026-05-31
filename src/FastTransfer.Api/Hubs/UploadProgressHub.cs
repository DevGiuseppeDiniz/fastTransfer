using Microsoft.AspNetCore.SignalR;

namespace FastTransfer.Api.Hubs;

public sealed class UploadProgressHub : Hub
{
    public Task JoinTransfer(string transferPublicId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(transferPublicId));
    }

    public Task LeaveTransfer(string transferPublicId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(transferPublicId));
    }

    public static string GroupName(string transferPublicId)
    {
        return $"transfer:{transferPublicId}";
    }
}
