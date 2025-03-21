using StackExchange.Redis;

using Shared.Interfaces.Cache;
using Shared.Models;
using Microsoft.Extensions.Configuration;


namespace Shared.Services.Cache;

/// <summary>
/// Provides caching services using Redis as the backend.
/// </summary>
/// <param name="redis">The Redis connection multiplexer.</param>
/// <param name="config">The application configuration.</param>
public class RedisService(IConnectionMultiplexer redis, IConfiguration config) : ICacheService
{
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly TimeSpan ttl = TimeSpan.Parse(config["REDIS:TTL"] ?? "00:30:00");

    // Helper methods for constructing Redis keys
    private static string GetSessionKey(string sessionId) => $"session:{sessionId}";
    private static string GetMessagesKey(string sessionId) => $"session:{sessionId}:messages";

    /// <inheritdoc/>
    public async Task CreateSessionAsync(string sessionId)
    {
        // Get database and create transaction
        var db = _redis.GetDatabase();
        var transaction = db.CreateTransaction();

        // Store session metadata
        _ = transaction.HashSetAsync(
            GetSessionKey(sessionId),
            [
                new("created_at", DateTime.UtcNow.Ticks),
                new("last_access", DateTime.UtcNow.Ticks)
                // TODO: Add a field for user ID when authentication is implemented
                // new("user_id", userId)
            ]
        );

        // Set initial expiration
        _ = transaction.KeyExpireAsync(
            GetSessionKey(sessionId),
            ttl);

        // Create empty messages sorted set
        _ = transaction.SortedSetAddAsync(
            GetMessagesKey(sessionId),
            []);

        await transaction.ExecuteAsync();
    }

    /// <inheritdoc/>
    public async Task AppendMessageAsync(string sessionId, ChatCompletionMessage message)
    {
        // Get database and create transaction
        var db = _redis.GetDatabase();
        var transaction = db.CreateTransaction();

        // Create a unique identifier
        var uniqueId = Guid.NewGuid().ToString();

        // Create sorted set entry
        var messageEntry = new SortedSetEntry(
            element: $"{message.Role}|{message.Content}|{uniqueId}",
            score: DateTime.UtcNow.Ticks
        );

        // Add message to sorted set
        _ = transaction.SortedSetAddAsync(
            GetMessagesKey(sessionId),
            [messageEntry]);

        // Update last access time
        _ = transaction.HashSetAsync(
            GetSessionKey(sessionId),
            "last_access",
            DateTime.UtcNow.Ticks);

        // Refresh expiration on each write
        _ = transaction.KeyExpireAsync(
            GetSessionKey(sessionId),
            ttl,
            ExpireWhen.HasExpiry);

        // Execute transaction
        await transaction.ExecuteAsync();
    }

    /// <inheritdoc/>
    public async Task<List<ChatCompletionMessage>> GetMessagesAsync(
        string sessionId,
        int maxCount = 10)
    {
        // Get database
        var db = _redis.GetDatabase();

        // Get messages from sorted set
        var entries = await db.SortedSetRangeByRankAsync(
            GetMessagesKey(sessionId),
            stop: maxCount - 1,
            order: Order.Descending);

        // Parse entries into ChatCompletionMessage objects
        return [.. entries.Select(entry =>
        {
            string[] parts = entry.ToString().Split('|', 3);
            return new ChatCompletionMessage {  Role = parts[0], Content = parts[1] };
        })];
    }

    /// <inheritdoc/>
    public async Task<bool> SessionExistsAsync(string sessionId)
    {
        // Check if session key exists
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(GetSessionKey(sessionId));
    }

    /// <inheritdoc/>
    public async Task UpdateLastAccessAsync(string sessionId)
    {
        // Update last access time
        var db = _redis.GetDatabase();
        await db.HashSetAsync(
            GetSessionKey(sessionId),
            "last_access",
            DateTime.UtcNow.Ticks);
    }

    /// <inheritdoc/>
    public async Task RebuildCacheAsync(
        string sessionId,
        IEnumerable<ChatCompletionMessage> messages)
    {
        // Get database and create transaction
        var db = _redis.GetDatabase();
        var transaction = db.CreateTransaction();

        // Delete existing data
        _ = transaction.KeyDeleteAsync(GetSessionKey(sessionId));

        // Recreate session
        await CreateSessionAsync(sessionId);

        // Add messages
        foreach (var msg in messages)
        {
            await AppendMessageAsync(sessionId, msg);
        }

        // Execute transaction
        await transaction.ExecuteAsync();
    }

    /// <inheritdoc/>
    public async Task ExpireSessionAsync(string sessionId, string timeToLive)
    {
        // Parse TTL
        if (!TimeSpan.TryParse(timeToLive, out TimeSpan ttl))
        {
            throw new ArgumentException("Invalid time-to-live format.");
        }

        // Set expiration time
        var db = _redis.GetDatabase();
        await db.KeyExpireAsync(GetSessionKey(sessionId), ttl);
    }
}