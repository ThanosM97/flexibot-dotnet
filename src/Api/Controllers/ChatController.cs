using Microsoft.AspNetCore.Mvc;

using Shared.Events;
using Shared.Models;
using Shared.Services;


namespace Api.Controllers;

/// <summary>
/// Controller responsible for handling chat requests.
/// </summary>
[ApiController]
[Route("chat")]
public class ChatController(RabbitMQPublisher publisher) : ControllerBase
{
    private readonly RabbitMQPublisher _publisher = publisher;

    /// <summary>
    /// Submits a chat question and initiates processing by publishing a chat prompted event.
    /// </summary>
    /// <param name="request">The chat request containing the session id and the prompt.</param>
    /// <returns>
    /// Returns a 202 Accepted response with a generated job ID if the request is successfully processed.
    /// </returns>
    /// <response code="202">Returns the job ID indicating the request has been accepted for processing.</response>
    /// <response code="400">If the <paramref name="request"/> is invalid.</response>
    [HttpPost]
    public async Task<IActionResult> SubmitQuestion([FromBody] ChatRequest request)
    {
        // Generate a unique job ID
        var jobId = Guid.NewGuid().ToString();

        // Publish a chat prompt event
        // IMPORTANT ISSUE: There is a race-condition introduced here. The response worker might start sending
        // chunks of the response before the client has subscribed to the job group.
        // POTENTIAL SOLUTION: Introduce a cache to store the jobs and publish the chat prompted event
        // after the client has subscribed to the job group in the hub (OnConnectedAsync).
        // TODO: Fix the race condition
        await _publisher.PublishAsync(
            new ChatPromptedEvent(jobId, request.SessionId, request.Prompt, request.PastMessagesIncluded, DateTime.UtcNow), "chat_prompted");

        // Return the job ID
        return Accepted(new { jobId });
    }
}