namespace TMS.DataAccessLayer.Entities;

public partial class PasswordResetToken
{
    public long PasswordResetTokenId { get; set; }

    public int AppUserId { get; set; }

    public byte[] TokenHash { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}
