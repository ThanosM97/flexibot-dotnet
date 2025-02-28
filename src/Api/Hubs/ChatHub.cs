using Microsoft.AspNetCore.SignalR;


namespace Api.Hubs;

/// <summary>
/// Represents a SignalR hub for managing chat-related communications.
/// </summary>
public class ChatHub : Hub
{
    /// <summary>
    /// Subscribes the connecting client to a specific job group based on the provided job ID.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to subscribe to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SubscribeToJob(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
    }

    /// <summary>
    /// Removes the connecting client from a specific job group based on the provided job ID.
    /// </summary>
    /// <param name="jobId">The unique identifier of the job to leave.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task LeaveJobGroup(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, jobId);
    }
}
