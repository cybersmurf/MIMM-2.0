using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MIMM.Backend.Data;
using MIMM.Backend.Hubs;
using MIMM.Shared.Dtos;
using MIMM.Shared.Entities;

namespace MIMM.Backend.Services;

public interface INotificationService
{
    Task<NotificationDto?> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<NotificationDto>> ListAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext dbContext,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _dbContext = dbContext;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<NotificationDto?> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Type = string.IsNullOrWhiteSpace(request.Type) ? "info" : request.Type,
            Title = request.Title,
            Message = request.Message,
            Link = request.Link,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(notification);

        await _hubContext.Clients.Group(GetGroupName(notification.UserId))
            .SendAsync("ReceiveNotification", dto, cancellationToken);

        _logger.LogInformation("Notification created for user {UserId} with Id {NotificationId}", notification.UserId, notification.Id);

        return dto;
    }

    public async Task<PagedResponse<NotificationDto>> ListAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => MapToDto(n))
            .ToListAsync(cancellationToken);

        return new PagedResponse<NotificationDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
        {
            return false;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var notifications = await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (notifications.Count == 0)
        {
            return 0;
        }

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return notifications.Count;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
        {
            return false;
        }

        _dbContext.Notifications.Remove(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static NotificationDto MapToDto(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type,
        Title = notification.Title,
        Message = notification.Message,
        Link = notification.Link,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt,
        ReadAt = notification.ReadAt
    };

    private static string GetGroupName(Guid userId) => $"user-{userId}";
}
