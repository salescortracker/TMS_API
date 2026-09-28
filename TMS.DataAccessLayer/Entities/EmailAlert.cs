namespace TMS.DataAccessLayer.Entities;

public partial class EmailAlert
{
    public long EmailAlertId { get; set; }

    public int? RecipientAppUserId { get; set; }

    public string RecipientLabel { get; set; } = null!;

    public string Subject { get; set; } = null!;

    public long? ActivityLogId { get; set; }

    public DateTime SentAt { get; set; }
}
