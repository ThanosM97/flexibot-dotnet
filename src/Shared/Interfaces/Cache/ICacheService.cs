using Shared.Models;


namespace Shared.Interfaces.Cache;

public interface ICacheService
{
    /// <summary>
    /// Creates a new session in the cache.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CreateSessionAsync(string sessionId);

    /// <summary>
    /// Appends a message to the session's message list in the cache.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="message">The message to append.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AppendMessageAsync(string sessionId, ChatCompletionMessage message);

    /// <summary>
    /// Retrieves messages from a session in the cache.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="maxCount">The maximum number of messages to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation, returning a list of messages.</returns>
    Task<List<ChatCompletionMessage>> GetMessagesAsync(string sessionId, int maxCount = 10);

    /// <summary>
    /// Checks if a session exists in the cache.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <returns>A task that represents the asynchronous operation, returning true if the session exists.</returns>
    Task<bool> SessionExistsAsync(string sessionId);

    /// <summary>
    /// Updates the last access time for a session in the cache.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateLastAccessAsync(string sessionId);

    /// <summary>
    /// Rebuilds the cache for a session by replacing its existing messages.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="messages">The collection of messages to store.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RebuildCacheAsync(string sessionId, IEnumerable<ChatCompletionMessage> messages);

    /// <summary>
    /// Sets an expiration time for a session in the cache.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the session.</param>
    /// <param name="timeToLive">The time-to-live (TTL) for the session.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the time-to-live format is invalid.</exception>
    Task ExpireSessionAsync(string sessionId, string timeToLive);
}