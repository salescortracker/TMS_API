namespace TMS.DataAccessLayer.Entities;

public partial class Notification
{
    public long NotificationId { get; set; }

    public int AppUserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Message { get; set; }

    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
