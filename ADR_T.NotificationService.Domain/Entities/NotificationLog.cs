using System.ComponentModel.DataAnnotations;

namespace ADR_T.NotificationService.Domain.Entities;
public class NotificationLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;
    public string? DetailsJson { get; set; }
    public bool IsProcessed { get; set; } = true;
}
